using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace needle.TimelineMixer
{
    public class TimelineGraphModificationManager : MonoBehaviour
    {
        public PlayableDirector Director;
        public List<TimelineMixHandler> Mixer;

        private int previousId;
        private List<TimelineMixHandler> previousMixers;
        

        protected virtual void OnValidate()
        {
            Inject();
        }

        public void Inject()
        {
            if (!DetectChange(out var requireRebuild)) return;
        }

        private bool DetectChange(out bool requireRebuildGraph)
        {
            if (!Director || !Director.playableGraph.IsValid())
            {
                previousId = -1;
                return false;
            }

            if (!Director || !Director.playableGraph.IsValid()) return false;
            var id = Director.playableGraph.GetHashCode();
            if (id == previousId) return false;
            previousId = id;

            return true;
        }
    }
}