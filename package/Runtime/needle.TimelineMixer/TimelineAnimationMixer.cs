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
        public abstract void OnUpdate(TimelineGraphModificationManager manager, AnimationLayerMixerPlayable mixer);

        public bool DidValidate { get; set; }
        
        protected virtual void OnValidate()
        {
            DidValidate = true;
        }

        // ReSharper disable once Unity.RedundantEventFunction
        protected virtual void OnEnable()
        {
            // this is just here for the enabled toggle
        }
    }
}