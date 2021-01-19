using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace needle.TimelineMixer
{
    public abstract class TimelineAnimationMixer : TimelineMixerBase, ITimelineMixerConnectable<AnimationLayerMixerPlayable>
    {
        public abstract Animator Animator { get; }

        /// <summary>
        /// Called when mixer playable gets injected into timeline playable graph
        /// </summary>
        public abstract void OnConnected(PlayableGraph graph, AnimationLayerMixerPlayable mixer);
        
        /// <summary>
        /// Called every frame to update weights
        /// </summary>
        public abstract void OnUpdate(TimelineGraphModificationManager manager, AnimationLayerMixerPlayable mixer);

    }
}