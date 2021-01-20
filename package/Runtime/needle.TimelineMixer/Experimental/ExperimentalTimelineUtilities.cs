using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace needle.TimelineMixer.Experimental
{
	public static class ExperimentalTimelineUtilities
	{
		public static void AddPlayables(PlayableDirector director, TimelineAsset asset, out List<(TrackAsset asset, Playable playable)> playables)
		{
			playables = new List<(TrackAsset asset, Playable playable)>();
			var graph = director.playableGraph;// PlayableGraph.Create(asset.name + "-MixInstance");
			foreach (var o in asset.GetOutputTracks())
			{
				var res = o.CreatePlayable(graph, director.gameObject);
				// if (res.IsValid()) Debug.Log(res);
				playables.Add((o, res));
			}
		}

		public static ScriptPlayable<TimelinePlayable> AddTimeline(this AnimationLayerMixerPlayable layerMixer,
			TimelineAsset asset, PlayableDirector director, out int index, bool autoBalance = true)
		{
			if (!director || !director.playableGraph.IsValid() || !asset)
			{
				index = -1;
				return ScriptPlayable<TimelinePlayable>.Null;
			}
			
			var tracks = asset.GetOutputTracks();

			// AddPlayables(director, asset, out var playables);
			//
			foreach (var trackAsset in tracks)
			{
				Debug.Log(trackAsset + " - " + trackAsset.duration);
				// playable.SetDuration(trackAsset.duration);
			}
			foreach (var trackAsset in (director.playableAsset as TimelineAsset).GetOutputTracks())
			{
				Debug.Log(trackAsset + " - " + trackAsset.duration);
				// playable.SetDuration(trackAsset.duration);
			}

			TimelineUtilities.TryFindTimelinePlayable(director.playableGraph, out var rootTimeline);
			// var timelineOutput = rootTimeline.GetOutput(0);
			// Debug.Log(rootTimeline.GetPlayableType() + ", " +  timelineOutput.IsValid());
			// var mixer = AnimationMixerPlayable.Create(director.playableGraph, 1);
			// mixer.ConnectInput(0, rootTimeline, 0);
			// timelineOutput.DisconnectInput(0);
			
			var timelinePlayable = TimelinePlayable.Create(director.playableGraph, tracks, director.gameObject, autoBalance, true);
			for (var i = 0; i < timelinePlayable.GetInputCount(); i++)
			{
				var input = timelinePlayable.GetInput(i);
				Debug.Log(input.GetPlayableType());
				// timelinePlayable.DisconnectInput(i);
				// mixer.AddInput(input, 0, 1);
			}

			// mixer.AddInput(timelinePlayable, 0, 1);

			// index = -1;
			index = layerMixer.AddInput(timelinePlayable, 0);
			return timelinePlayable;
		}
	}
}