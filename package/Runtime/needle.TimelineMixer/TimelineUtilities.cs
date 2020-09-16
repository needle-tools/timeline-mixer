using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace needle.TimelineMixer
{
    public static class TimelineUtilities
    {
        public static AnimationClipPlayable AddClip(this AnimationLayerMixerPlayable layerMixer, PlayableGraph graph, AnimationClip clip,  out int index)
        {
            var playable = AnimationClipPlayable.Create(graph, clip);
            index = layerMixer.AddInput(playable, 0);
            return playable;
        }
        
        public static bool TryInjectMixer(this PlayableDirector dir, Playable timelinePlayable, Animator animator,
            out AnimationLayerMixerPlayable mixerPlayable)
        {
            mixerPlayable = AnimationLayerMixerPlayable.Null;

            if (timelinePlayable.IsValid() == false)
            {
                Debug.LogError("Timeline Playable is not valid");
                return false;
            }

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
            if (!TryGetTracks(dir, tracks))
            {
                Debug.LogError("No AnimationTracks found", dir);
                return false;
            }

            var animatorIndex = 0;
            var found = false;
            // try find the timeline graph index for this animator
            // we do this by looping bindings for all animation tracks
            // and check if the binding maps to the animator we want to inject a mixer for
            foreach (var track in tracks)
            {
                if (found) break;
                if (track.isEmpty || track.muted || !track.hasClips) continue;
                var trackOutputs = track.outputs;
                foreach (var to in trackOutputs)
                {
                    var trackBinding = dir.GetGenericBinding(to.sourceObject);
                    if (trackBinding && trackBinding == animator)
                    {
                        found = true;
                        break;
                    }

                    animatorIndex++;
                }
            }

            if (!found)
            {
                Debug.LogWarning("Animator " + animator + " is not bound to " + dir, dir);
                return false;
            }

            Debug.Log("found: " + animatorIndex);

            var playable = timelinePlayable.GetInput(animatorIndex);
            var prevOutput = playable.GetOutput(0);
            var prevInput = prevOutput.GetInput(animatorIndex);
            // Debug.Log(prevOutput.GetPlayableType() + ", " + prevInput.GetPlayableType());
            // return false;
            mixerPlayable = AnimationLayerMixerPlayable.Create(dir.playableGraph, 1);
            // var pWeight = o.GetInputWeight(0);
            prevOutput.DisconnectInput(animatorIndex);
            prevOutput.ConnectInput(animatorIndex, mixerPlayable, 0);
            mixerPlayable.ConnectInput(0, prevInput, 0, 1);
            return mixerPlayable.IsValid();
        }

        public static bool TryGetTracks(PlayableDirector dir, List<TrackAsset> tracksCache)
        {
            if (!dir)
            {
                Debug.LogError("PlayableDirector is null");
                return false;
            }

            if (dir.playableAsset && dir.playableAsset is TimelineAsset timelineAsset)
            {
                var outputTracks = timelineAsset.GetOutputTracks();
                // var tracks = outputTracks
                //     .Where(x => x is AnimationTrack)
                //     .Cast<AnimationTrack>();
                tracksCache.AddRange(outputTracks);
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