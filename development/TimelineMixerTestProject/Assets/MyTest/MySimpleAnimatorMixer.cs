using System.Collections.Generic;
using System.Linq;
using needle.TimelineMixer;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MyTest
{
    public class MySimpleAnimatorMixer : MonoBehaviour
    {
        public PlayableDirector dir;

        public AnimationClip clip;

        private List<AnimationLayerMixerPlayable> mixers;
        [SerializeField] private List<Animator> boundAnimators = new List<Animator>();


        private List<AnimationMixerPlayable> injectedMixers = new List<AnimationMixerPlayable>();
        private List<AnimationClipPlayable> clips = new List<AnimationClipPlayable>();

        public int index = 0;
        [Range(0, 1)] public float weight = 1;

        public bool OnlyAddWeight = false;


        private void OnValidate()
        {
            UpdateWeights();
        }

        [ContextMenu(nameof(DisconnectAll))]
        private void DisconnectAll()
        {
            foreach (var m in mixers)
            {
                m.DisconnectInput(0);
                // dir.playableGraph.Disconnect(m, 0);
            }
        }

        [ContextMenu(nameof(InjectMixers))]
        private void InjectMixers()
        {
            injectedMixers.Clear();
            foreach (var amp in mixers)
            {
                // amp.GetOutput(0).DisconnectInput(0);
                var inj = AnimationMixerPlayable.Create(dir.playableGraph, 1);
                var pl = AnimationClipPlayable.Create(dir.playableGraph, clip);
                clips.Add(pl);
                inj.AddInput(pl, 0);
                // inj.ConnectInput(0, amp, 0, 1);
                var o = amp.GetOutput(0);
                var prevInput = o.GetInput(0);
                o.DisconnectInput(0);
                o.ConnectInput(0, inj, 0);
                inj.ConnectInput(0, prevInput, 0);
                injectedMixers.Add(inj);

                UpdateWeights();
            }
        }

        private void UpdateWeights()
        {
            foreach (var clipPlayable in clips)
            {
                if (clipPlayable.IsValid())
                    clipPlayable.SetTime(0);
            }

            if (injectedMixers == null) return;
            if (index >= 0 && index < injectedMixers.Count)
            {
                var m = injectedMixers[index];
                if (m.IsValid())
                {
                    Debug.Log(m + ": " + weight + " -> " + boundAnimators[index] + ", " + m.CanSetWeights() + ", " + m.GetInputWeight(0));
                    for (var i = 0; i < m.GetInputCount(); i++)
                    {
                        var w = i % 2 == 0 ? weight : 1 - weight;
                        if (OnlyAddWeight && i > 0) w = weight;
                        m.SetInputWeight(i, w);
                    }
                }
            }
        }

        public bool InjectOnStart = true;

        private void Start()
        {
            mixers = new List<AnimationLayerMixerPlayable>();
            for (var i = 0; i < dir.playableGraph.GetOutputCount(); i++)
            {
                var p = dir.playableGraph.GetRootPlayable(i);
                if (!p.IsValid()) continue;
                Debug.Log(p.GetPlayableType());
                Debug.Log("OUTPUT: " + i + " = " + p.GetPlayableType() + ", outputs: " + p.GetOutputCount() + ", inputs: " + p.GetInputCount());
                Traverse(p, mixers);
            }

            var arr = dir.playableAsset.outputs.ToArray();
            for (var i = 0; i < arr.Length; i++)
            {
                var entry = arr[i];

                if (entry.outputTargetType == typeof(Animator))
                {
                    var binding = dir.GetGenericBinding(entry.sourceObject);
                    var b2 = dir.GetReferenceValue(new PropertyName(entry.streamName), out var valid);
                    Debug.Log("BINDING: " + i + ", " + binding + ", " + b2 + " valid=" + valid + "  " + entry.streamName + " - " + entry.sourceObject +
                              ", " + binding);
                    boundAnimators.Add(binding as Animator);
                }
                else
                {
                    var binding = dir.GetGenericBinding(entry.sourceObject);
                    var b2 = dir.GetReferenceValue(new PropertyName(entry.streamName), out var valid);
                    Debug.Log("BINDING: " + i + " is " + entry.outputTargetType + " for " + b2 + ", " + entry.streamName + ", " + binding);
                }
            }

            if (InjectOnStart) InjectMixers();

            Debug.Log("-----------");
            if (TimelineUtilities.TryFindTimelinePlayable(dir.playableGraph, out var timeline))
            {
                Debug.Log(timeline.GetPlayableType() + ", " + timeline.GetType());
                if (TimelineUtilities.TryInjectMixer(dir, timeline, GetComponent<Animator>(), out var mixer))
                {
                    Debug.Log("INJECTED MIXER" + mixer);
                }
            }
        }

        private void Traverse(Playable pl, List<AnimationLayerMixerPlayable> mixers, bool addedMixer = false)
        {
            for (var i = 0; i < pl.GetInputCount(); i++)
            {
                var p = pl.GetInput(i);
                if (!p.IsValid()) continue;
                // var w = p.GetInputWeight(0);
                Debug.Log(p.GetPlayableType() + " - " + i + " output: " + p.GetOutputCount());

                var t = p.GetPlayableType();
                if (t == typeof(AnimationLayerMixerPlayable))
                {
                    var amp = (AnimationLayerMixerPlayable) p;
                    if (!addedMixer) // only add first found mixer per playable
                    {
                        mixers.Add(amp);
                    }

                    addedMixer = true;
                }
                else Debug.Log(t + ", " + p.GetInputCount());

                Traverse(p, mixers, addedMixer);
            }
        }

    }
}