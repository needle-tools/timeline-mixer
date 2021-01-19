using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace needle.TimelineMixer.Experimental
{
	public class TimelineMixer : TimelineAnimationMixer
	{
		public Animator animator;
		public PlayableDirector Director;
		public TimelineAsset Asset;
		public float Weight;
		private int index;

		private PlayableGraph myGraph;
		private ScriptPlayable<TimelinePlayable> timeline;

		public override Animator Animator => animator;
		public override void OnConnected(PlayableGraph graph, AnimationLayerMixerPlayable mixer)
		{
			myGraph = ExperimentalTimelineUtilities.GetGraph(Director, Asset);
			timeline  = mixer.AddTimeline(Asset, Director, myGraph, out index);
		}

		public override void OnUpdate(TimelineGraphModificationManager manager, AnimationLayerMixerPlayable mixer)
		{
			myGraph.Evaluate(Time.deltaTime);
			myGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
			mixer.SetInputWeight(0, 1 - Weight);
			mixer.SetInputWeight(index, Weight);
			if (timeline.GetTime() > 1.5) timeline.SetTime(0);
		}
	}
}