using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace needle.TimelineMixer
{
    public abstract class TimelineAnimationMixer : MonoBehaviour
    {
        public abstract Animator Animator { get; }
        public bool RequestGraphRebuild { get; set; }

        
        public abstract void OnConnected(PlayableGraph graph, AnimationLayerMixerPlayable mixer);
        public abstract void OnUpdate(AnimationLayerMixerPlayable mixer);

        public bool DidValidate { get; set; }
        
        protected virtual void OnValidate()
        {
            DidValidate = true;
        }
    }
}