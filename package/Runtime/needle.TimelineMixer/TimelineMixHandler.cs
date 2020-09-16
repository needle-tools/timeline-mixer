using UnityEngine;
using UnityEngine.Animations;

namespace needle.TimelineMixer
{
    public abstract class TimelineMixHandler : MonoBehaviour
    {
        public abstract Animator Animator { get; }
        public abstract void OnUpdate(AnimationMixerPlayable mixer);
    }
}