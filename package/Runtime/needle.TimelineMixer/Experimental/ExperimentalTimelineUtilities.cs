using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace needle.TimelineMixer.Experimental
{
	public static class ExperimentalTimelineUtilities
	{
		public static PlayableGraph GetGraph(PlayableDirector director, TimelineAsset asset)
		{
			var prev = director.playableAsset;
			director.RebuildGraph();
			var graph = director.playableGraph;
			var o = new PlayableOutput();
			// var myGraph = PlayableGraph.Create();
			// var root = graph.GetRootPlayable(0);
			// myGraph.Connect(root, 0, myGraph.GetRootPlayable(0), 0);
			director.playableAsset = prev;
			director.RebuildGraph();
			return graph;
		}
		
		public static ScriptPlayable<TimelinePlayable> AddTimeline(this AnimationLayerMixerPlayable layerMixer, 
			TimelineAsset asset, PlayableDirector director, PlayableGraph graph, out int index, bool autoBalance = true)
		{
			if (!director)
			{
				index = -1;
				return ScriptPlayable<TimelinePlayable>.Null;
			}
			
			var timelinePlayable = TimelinePlayable.Create(graph, asset.GetOutputTracks(), director.gameObject, autoBalance, true);
			index = layerMixer.AddInput(timelinePlayable, 0);
			return timelinePlayable;
		}

		public static ScriptPlayable<TimelinePlayable> AddTimeline(this AnimationLayerMixerPlayable layerMixer, 
			TimelineAsset asset, GameObject timelineRoot, out int index, bool autoBalance = true)
		{
			if (!timelineRoot || !asset)
			{
				index = -1;
				return ScriptPlayable<TimelinePlayable>.Null;
			}
		
			var g = PlayableGraph.Create("MyGraph");
			
			
			var timelinePlayable = TimelinePlayable.Create(g, asset.GetOutputTracks(), timelineRoot, autoBalance, true);
			index = layerMixer.AddInput(timelinePlayable, 0);
			return timelinePlayable;
		}
	}
}