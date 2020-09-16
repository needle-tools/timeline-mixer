using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace needle.TimelineMixer
{
    public class SimpleAnimationClipMixer : TimelineAnimationMixer
    {
        public Animator _animator;
        public AnimationClip Clip;

        [Range(0, 1)] public float Weight01;

        public bool Additive = false;

        [SerializeField] private bool proceedTimeInEditMode = true;

        public override Animator Animator => _animator;

        private AnimationClipPlayable playable;
        private int index;
        
        // this is just here for the toggle
        // ReSharper disable once Unity.RedundantEventFunction
        private void OnEnable()
        {
        }

        public override void OnConnected(PlayableGraph graph, AnimationLayerMixerPlayable mixer)
        {
            playable = AnimationClipPlayable.Create(graph, Clip);
            index = mixer.AddInput(playable, 0);
        }

        public override void OnUpdate(TimelineGraphModificationManager manager, AnimationLayerMixerPlayable mixer)
        {
            if (!playable.IsValid()) return;
            if (proceedTimeInEditMode && !Application.isPlaying)
            {
                playable.SetTime(playable.GetTime() + Time.deltaTime);
            }

            mixer.SetInputWeight(index, Weight01);
            if (Additive) mixer.SetInputWeight(0, 1);
            else mixer.SetInputWeight(0, 1 - Weight01);
        }
    }
}