using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace needle.TimelineMixer
{
    [ExecuteInEditMode]
    public class TimelineGraphModificationManager : MonoBehaviour
    {
        [SerializeField] private PlayableDirector Director;

        [SerializeField] private List<TimelineAnimationMixer> Mixer = default;

        public void Add(TimelineAnimationMixer mixer)
        {
            if (!mixer) return;
            if (Mixer.Contains(mixer)) return;
            Mixer.Add(mixer);
        }

        public void Remove(TimelineAnimationMixer mixer)
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
                Director.RebuildGraph();
                UpdateIdIfChanged();
            }
            else if (!DetectGraphChanged()) return;

            InternalInjectNow();
        }

        private int previousId;
        private readonly List<TimelineAnimationMixer> previousMixers = new List<TimelineAnimationMixer>();
        private readonly List<Animator> previousAnimators = new List<Animator>();
        private readonly List<bool> previousStates = new List<bool>();

        private readonly List<(TimelineAnimationMixer handler, AnimationLayerMixerPlayable playable)> injectedMixers =
            new List<(TimelineAnimationMixer, AnimationLayerMixerPlayable)>();


        protected virtual void OnValidate()
        {
            if (!Director) Director = GetComponent<PlayableDirector>();
            Inject();
        }

        private void OnEnable()
        {
            Inject();
        }

        private void OnDisable()
        {
            if (injectedMixers.Count <= 0) return;
            injectedMixers.Clear();
            Director.RebuildGraph();
        }

        private void Update()
        {
            if (!Director.playableGraph.IsValid()) return;

            if (!Application.isPlaying)
            {
                Inject();

                var validated = false;
                foreach (var mixer in Mixer)
                {
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
            var mixersChanged = Mixer.Count != previousMixers.Count;
            for (var i = 0; i < Mixer.Count && i < previousMixers.Count; i++)
            {
                var current = Mixer[i];
                var prev = previousMixers[i];
                if (current != prev)
                    mixersChanged = true;
                if (current && current.RequestGraphRebuild)
                {
                    mixersChanged = true;
                    current.RequestGraphRebuild = false;
                }
                else if (current && prev)
                {
                    var state = previousStates[i];
                    if (current.Animator != prev.Animator)
                        mixersChanged = true;
                    else if (current.enabled != state)
                        mixersChanged = true;
                }
            }

            if (!mixersChanged) return false;

            previousMixers.Clear();
            previousAnimators.Clear();
            previousStates.Clear();
            foreach (var mix in Mixer)
            {
                previousMixers.Add(mix);
                previousAnimators.Add(mix ? mix.Animator : null);
                previousStates.Add(mix.enabled);
            }

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
                var mixer = Mixer[index];
                if (!mixer || !mixer.enabled) continue;
                var animator = mixer.Animator;
                if (!animator)
                    continue;
                if (Director.TryInjectMixer(timelinePlayable, animator, out var mixerPlayable))
                {
                    injectedMixers.Add((mixer, mixerPlayable));
                    try
                    {
                        mixer.OnConnected(Director.playableGraph, mixerPlayable);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
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
    }
}