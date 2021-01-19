using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace needle.TimelineMixer.Samples
{
    public class MultiClipMixer : TimelineAnimationMixer
    {
        [Range(0, 1)]
        public float Blend = 0;
    
        public Animator _animator;

        public override Animator Animator => _animator;

        public List<AnimationClip> Clips;

        private readonly List<(AnimationClipPlayable playable, int index, AnimationClip clip)> data = new List<(AnimationClipPlayable playable, int index, AnimationClip clip)>();
    
        public override void OnConnected(PlayableGraph graph, AnimationLayerMixerPlayable mixer)
        {
            data.Clear();
            foreach (var clip in Clips)
            {
                var playable = mixer.AddClip(graph, clip, out var index);
                data.Add((playable, index, clip));
            }
        }

        public override void OnUpdate(TimelineGraphModificationManager manager, AnimationLayerMixerPlayable mixer)
        {
            if (Clips == null || Clips.Count == 0) return;

            var total = Blend * data.Count;
            // first input is original timeline (index 0)
            mixer.SetInputWeight(0, Mathf.Clamp01(1 - total));
        
            for (var i = 0; i < data.Count; i++)
            {
                var (playable, index, clip) = data[i];
                if (!Application.isPlaying)
                {
                    var newTime = playable.GetTime() + Time.deltaTime;
                    playable.SetTime(newTime);
                }
                if (playable.GetTime() > clip.length) playable.SetTime(0);
                var weight = (index) - total;
                weight = 1 - Mathf.Abs(weight);
                mixer.SetInputWeight(index, weight);
            }

        }
    }
}
