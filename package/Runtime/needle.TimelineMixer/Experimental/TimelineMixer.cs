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
		public float Duration = 3;

		// private PlayableGraph myGraph;
		private ScriptPlayable<TimelinePlayable> timeline;

		public override Animator Animator => animator;
		public override void OnConnected(PlayableGraph graph, AnimationLayerMixerPlayable mixer)
		{
			timeline = mixer.AddTimeline(Asset, Director, out index);
			Debug.Log("Duration: " + timeline.GetDuration());
		}

		public override void OnUpdate(TimelineGraphModificationManager manager, AnimationLayerMixerPlayable mixer)
		{
			// myGraph.Evaluate(Time.deltaTime);
			// myGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
			timeline.SetTime(timeline.GetTime()+Time.deltaTime);
			if (timeline.GetTime() > Duration)
			{
				timeline.SetTime(0);
				Debug.Log(timeline.GetTime()); 
			}
			if (index < 0) return;
			mixer.SetInputWeight(0, 1 - Weight);
			mixer.SetInputWeight(index, Weight);

		}
	}
}