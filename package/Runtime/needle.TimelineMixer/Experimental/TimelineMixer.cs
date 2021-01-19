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

		// private PlayableGraph myGraph;
		private ScriptPlayable<TimelinePlayable> timeline;

		public override Animator Animator => animator;
		public override void OnConnected(PlayableGraph graph, AnimationLayerMixerPlayable mixer)
		{
			// myGraph = ExperimentalTimelineUtilities.GetGraph(Director, Asset);
			timeline  = mixer.AddTimeline(Asset, Director, Director.playableGraph, out index);
		}

		public override void OnUpdate(TimelineGraphModificationManager manager, AnimationLayerMixerPlayable mixer)
		{
			if (!Application.isPlaying)
			{
				var newTime = timeline.GetTime() + Time.deltaTime * .5f;
				timeline.SetTime(newTime);
				// timeline.GetBehaviour().PrepareFrame(timeline, new FrameData());
			}
			mixer.SetInputWeight(0, 1 - Weight);
			mixer.SetInputWeight(index, Weight);
			if (timeline.GetTime() > 1.5) timeline.SetTime(0);
		}
	}
}