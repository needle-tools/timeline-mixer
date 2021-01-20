using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace needle.TimelineMixer
{
    [ExecuteInEditMode]
    public class TimelineGraphModificationManager : MonoBehaviour
    {
        [SerializeField] private PlayableDirector Director;

        public bool AutoCollectMixerAtStart = true;

        [SerializeField] private List<TimelineMixerBase> Mixer = default;

        public void Add(TimelineMixerBase mixer)
        {
            if (!mixer) return;
            if (Mixer.Contains(mixer)) return;
            Mixer.Add(mixer);
        }

        public void Remove(TimelineMixerBase mixer)
        {
            if (!mixer) return;
            if (!Mixer.Contains(mixer)) return;
            Mixer.Remove(mixer);
        }

        public void Inject()
        {
            if (!Director || !Director.playableGraph.IsValid()) return;
            var mixersChanged = DetectMixersChanged();
            if (mixersChanged)
            {
                var time = Director.time;
                Director.RebuildGraph();
                Director.time = time;
                UpdateIdIfChanged();
            }
            else if (!DetectGraphChanged()) return;

            InternalInjectNow();
        }

        private bool requireEvaluate;
        public void RequireTimelineEvaluate() => requireEvaluate = true;
        public bool TimelineIsPlaying() => Director && Director.playableGraph.IsValid() && Director.playableGraph.IsPlaying();

        private int previousId;

        private class State
        {
            public Object instance;
            public bool enabled;
            public Animator animator;
        }
        
        private readonly List<State> previousMixers = new List<State>();

        private readonly List<(TimelineAnimationMixer handler, AnimationLayerMixerPlayable playable)> injectedMixers =
            new List<(TimelineAnimationMixer, AnimationLayerMixerPlayable)>();


        protected virtual void OnValidate()
        {
            if (!Director) Director = GetComponent<PlayableDirector>();
            Inject();
        }

        private void Start()
        {
            if (AutoCollectMixerAtStart)
            {
                FindMixerInScene();
            }
        }

        private void OnEnable()
        {
            if (AutoCollectMixerAtStart && !Application.isPlaying && Application.isEditor)
                FindMixerInScene();
            Inject();
        }

        private void OnDisable()
        {
            if (injectedMixers.Count <= 0) return;
            injectedMixers.Clear();
            var time = Director.time;
            Director.RebuildGraph();
            Director.time = time;
        }

        private void Update()
        {
            if (!Director || !Director.playableGraph.IsValid()) return;

            Inject();
            
            if (!Application.isPlaying)
            {
                var validated = false;
                foreach (var mixer in Mixer)
                {
                    if (!mixer) continue;
                    validated |= mixer.DidValidate;
                    mixer.DidValidate = false;
                }

                if (validated)
                {
                    Director.playableGraph.Evaluate(0);
                }
            }

            for (var index = 0; index < injectedMixers.Count; index++)
            {
                var entry = injectedMixers[index];
                entry.handler.OnUpdate(this, entry.playable);
            }

            if (requireEvaluate)
            {
                requireEvaluate = false;
                Director.Evaluate();
            }
        }


        private bool DetectGraphChanged()
        {
            if (!Director || !Director.playableGraph.IsValid())
            {
                previousId = -1;
                return false;
            }

            if (!Director || !Director.playableGraph.IsValid()) return false;
            return UpdateIdIfChanged();
        }

        private bool UpdateIdIfChanged()
        {
            var id = Director.playableGraph.GetHashCode();
            if (id == previousId) return false;
            previousId = id;
            return true;
        }

        private bool DetectMixersChanged()
        {
            if(Mixer == null) Mixer = new List<TimelineMixerBase>();
            var mixersChanged = Mixer.Count != previousMixers.Count;
            for (var i = 0; i < Mixer.Count; i++)
            {
                if (i >= previousMixers.Count) previousMixers.Add(new State());
                var current = Mixer[i];
                var prev = previousMixers[i];
                if (current != prev.instance)
                {
                    prev.instance = current;
                    mixersChanged = true;
                }
                if (current && current.RequestGraphRebuild)
                {
                    mixersChanged = true;
                    current.RequestGraphRebuild = false;
                }
                else if (current && prev.instance)
                {
                    if (current is TimelineAnimationMixer ta && ta.Animator != prev.animator)
                    {
                        prev.animator = ta.Animator;
                        mixersChanged = true;
                    }
                    
                    if (current.enabled != prev.enabled)
                    {
                        prev.enabled = current.enabled;
                        mixersChanged = true;
                    }
                }
            }
            if (!mixersChanged) return false;
            return true;
        }

        private void InternalInjectNow()
        {
            if (!enabled) return;

            if (!TimelineUtilities.TryFindTimelinePlayable(Director.playableGraph, out var timelinePlayable))
            {
                Debug.LogError("Failed finding timeline playable", this);
                return;
            }


            injectedMixers.Clear();
            for (var index = 0; index < Mixer.Count; index++)
            {
                var entry = Mixer[index];
                if (!entry || !entry.enabled) continue;
                if (entry is TimelineAnimationMixer animatorMixer)
                {
                    var animator = animatorMixer.Animator;
                    if (!animator)
                        continue;
                    if (Director.TryInjectMixer(timelinePlayable, animator, out var mixerPlayable))
                    {
                        injectedMixers.Add((animatorMixer, mixerPlayable));
                        try
                        {
                            animatorMixer.OnConnected(Director.playableGraph, mixerPlayable);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                }
            }
        }

        [ContextMenu(nameof(RequestRebuildGraph))]
        private void RequestRebuildGraph()
        {
            if (Director)
                Director.RebuildGraph();
        }

        [ContextMenu(nameof(FindMixerInHierarchy))]
        private void FindMixerInHierarchy()
        {
            var mixers = GetComponentsInChildren<TimelineMixerBase>(true);
            foreach (var mix in mixers) this.Add(mix);
        }

        [ContextMenu(nameof(FindMixerInScene))]
        private void FindMixerInScene()
        {
            var mixers = FindObjectsOfType<TimelineMixerBase>();
            foreach (var mix in mixers) this.Add(mix);
        }
    }
}