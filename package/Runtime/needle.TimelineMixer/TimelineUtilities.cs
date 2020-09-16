using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace needle.TimelineMixer
{
    public static class TimelineUtilities
    {
        public static bool TryInjectMixer(this PlayableDirector dir, Animator animator, out AnimationLayerMixerPlayable mixerPlayable)
        {
            mixerPlayable = AnimationLayerMixerPlayable.Null;
            if (!dir)
            {
                Debug.LogError("PlayableDirector is null");
                return false;
            }
            if (!animator)
            {
                Debug.LogError("Animator is null");
                return false;
            }
            if (TryFindTimelinePlayable(dir.playableGraph, out var timelinePlayable))
            {
                return dir.TryInjectMixer(timelinePlayable, animator, out mixerPlayable);
            }
            mixerPlayable = AnimationLayerMixerPlayable.Null;
            return false;
        }
        
        public static bool TryInjectMixer(this PlayableDirector dir, Playable timelinePlayable, Animator animator, out AnimationLayerMixerPlayable mixerPlayable)
        {
            mixerPlayable = AnimationLayerMixerPlayable.Null;
            if (!dir)
            {
                Debug.LogError("PlayableDirector is null");
                return false;
            }
            if (!dir.playableAsset)
            {
                Debug.LogError("PlayableDirector has no TimelineAsset assigned", dir);
                return false;
            }
            if (!animator)
            {
                Debug.LogError("Animator is null");
                return false;
            }

            var tracks = new List<TrackAsset>();
            if (!TryGetAnimationTracks(dir, tracks))
            {
                Debug.LogError("No AnimationTracks found", dir);
                return false;
            }

            Debug.Log(animator.name);
            var outputs = dir.playableAsset.outputs;
            int animatorIndex = 0, animatorOutputsIndex = 0;
            var found = false;
            // try find the timeline graph index for this animator
            // we do this by looping bindings for all animation tracks
            // and check if the binding maps to the animator we want to inject a mixer for
            foreach (var output in outputs)
            {
                var binding = dir.GetGenericBinding(output.sourceObject);
                if (!binding || binding.GetType() != typeof(Animator)) continue;
                if (binding == animator)
                {
                    found = true;
                    break;
                }

                // muted and tracks without animation clips dont show up in the graph, therefore we need to skip these
                var track = tracks[animatorOutputsIndex];
                if (track.hasClips && !track.muted)
                    animatorIndex++;
                animatorOutputsIndex++;
            }

            if (!found)
            {
                Debug.LogError("Animator " + animator + " is not bound to " + dir, dir);
                return false;
            }

            // Debug.Log("found: " + animatorIndex);

            var playable = timelinePlayable.GetInput(animatorIndex);
            mixerPlayable = AnimationLayerMixerPlayable.Create(dir.playableGraph, 1);

            var o = playable.GetOutput(0);
            var prevInput = o.GetInput(0);
            // var pWeight = o.GetInputWeight(0);
            o.DisconnectInput(0);
            o.ConnectInput(0, mixerPlayable, 0);
            mixerPlayable.ConnectInput(0, prevInput, 0, 1);
            return mixerPlayable.IsValid();
        }

        public static bool TryGetAnimationTracks(PlayableDirector dir, List<TrackAsset> tracksCache)
        {
            if (!dir)
            {
                Debug.LogError("PlayableDirector is null");
                return false;
            }
            if (dir.playableAsset && dir.playableAsset is TimelineAsset timelineAsset)
            {
                var outputTracks = timelineAsset.GetOutputTracks();
                var tracks = outputTracks
                    .Where(x => x is AnimationTrack)
                    .Cast<AnimationTrack>();
                tracksCache.AddRange(tracks);
                return true;
            }

            return tracksCache.Count > 0;
        }

        public static bool TryFindTimelinePlayable(PlayableGraph graph, out Playable timelinePlayable)
        {
            timelinePlayable = Playable.Null;
            
            if (!graph.IsValid())
            {
                Debug.LogError("Cant find TimelinePlayable: PlayableGraph is not valid");
                return false;
            }
            
            bool TraverseSearchTimelinePlayable(Playable pl, ref Playable timeline)
            {
                if (timeline.IsValid()) return true;
                if (pl.IsValid() && pl.GetPlayableType() == typeof(TimelinePlayable))
                {
                    timeline = pl;
                    return true;
                }

                for (var k = 0; k < pl.GetInputCount(); k++)
                {
                    var input = pl.GetInput(k);
                    if (TraverseSearchTimelinePlayable(input, ref timeline))
                        break;
                }

                return timeline.IsValid();
            }

            for (var i = 0; i < graph.GetOutputCount(); i++)
            {
                var rootOutput = graph.GetRootPlayable(i);
                if (TraverseSearchTimelinePlayable(rootOutput, ref timelinePlayable))
                    break;
            }

            return timelinePlayable.IsValid();
        }
    }
}
