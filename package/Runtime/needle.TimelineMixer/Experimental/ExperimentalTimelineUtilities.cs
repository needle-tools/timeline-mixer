using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
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

			// AddPlayables(director, asset, out var playables);
			//
			// foreach (var (trackAsset, playable) in playables)
			// {
			// 	Debug.Log(trackAsset + " - " + trackAsset.duration + ", PlayableIsValid? " + playable.IsValid());
			// 	// playable.SetDuration(trackAsset.duration);
			// }
			
			var tracks = asset.GetOutputTracks();
			var timelinePlayable = TimelinePlayable.Create(director.playableGraph, tracks, director.gameObject, autoBalance, true);
			var mixer = AnimationMixerPlayable.Create(director.playableGraph, timelinePlayable.GetInputCount());
			for (var i = 0; i < timelinePlayable.GetInputCount(); i++)
			{
				var input = timelinePlayable.GetInput(i);
				Debug.Log(input.GetPlayableType());
				// timelinePlayable.DisconnectInput(i);
				// mixer.AddInput(input, 0, 1);
			}

			mixer.AddInput(timelinePlayable, 0, 1);

			index = layerMixer.AddInput(mixer, 0);
			return timelinePlayable;
		}
	}
}