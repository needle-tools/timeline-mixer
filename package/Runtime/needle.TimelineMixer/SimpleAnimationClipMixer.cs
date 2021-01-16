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
        public bool Loop = true;

        [SerializeField] private bool proceedTimeInEditMode = true;

        public override Animator Animator => _animator;

        private AnimationClipPlayable playable;
        private int index;

        private AnimationClip prevClip;

        protected override void OnValidate()
        {
            base.OnValidate();
            RequestGraphRebuild = prevClip != Clip;
            prevClip = Clip;
        }

        public override void OnConnected(PlayableGraph graph, AnimationLayerMixerPlayable mixer)
        {
            playable = mixer.AddClip(graph, Clip, out index);
        }

        public override void OnUpdate(TimelineGraphModificationManager manager, AnimationLayerMixerPlayable mixer)
        {
            if (!playable.IsValid()) return;
            if (!Clip) return;
            
            if (proceedTimeInEditMode && !Application.isPlaying)
            {
                var newTime = playable.GetTime() + Time.deltaTime;
                playable.SetTime(newTime);
            }

            if (Loop && playable.GetTime() > Clip.length) playable.SetTime(0);

            mixer.SetInputWeight(index, Weight01);
            if (Additive) mixer.SetInputWeight(0, 1);
            else mixer.SetInputWeight(0, 1 - Weight01);
        }
    }
}