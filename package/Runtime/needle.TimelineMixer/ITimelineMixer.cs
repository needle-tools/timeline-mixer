using UnityEngine;
using UnityEngine.Playables;

namespace needle.TimelineMixer
{
	public interface ITimelineMixer
	{
		bool RequestGraphRebuild { get;}
	}

	public interface ITimelineMixerConnectable<T> : ITimelineMixer where T : IPlayable
	{
		void OnConnected(PlayableGraph graph, T playable);
		void OnUpdate(TimelineGraphModificationManager manager, T mixer);
	}

	public abstract class TimelineMixerBase : MonoBehaviour, ITimelineMixer
	{
		public bool RequestGraphRebuild { get; set; }
		
		public bool DidValidate { get; set; }
        
		protected virtual void OnValidate()
		{
			DidValidate = true;
		}

		// ReSharper disable once Unity.RedundantEventFunction
		protected virtual void OnEnable()
		{
			// this is just here for the enabled toggle
		}
	}
}