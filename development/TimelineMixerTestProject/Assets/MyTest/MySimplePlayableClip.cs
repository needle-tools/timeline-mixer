using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MyTest
{
    public class MySimplePlayableClip : MonoBehaviour
    {
        public Animator m_Animator;
        public AnimationClip Clip;
        private PlayableGraph m_Graph;
        private MyPlayable m_Playable;

        public float Weight = 1;

        private void OnValidate()
        {
            if (Application.isPlaying && m_Playable != null)
            {
                m_Playable.SetWeight(Weight);
            }
        }

        private void Start()
        {
            m_Animator = GetComponent<Animator>();
            m_Graph = PlayableGraph.Create();
            m_Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            
            MyPlayable template = new MyPlayable();
            var playable = ScriptPlayable<MyPlayable>.Create(m_Graph, template, 1);
            m_Playable = playable.GetBehaviour();
            // m_Playable.onDone += OnPlayableDone;
            
            // AnimationPlayableUtilities.PlayMixer()

            m_Playable.Setup(Clip);
            AnimationPlayableUtilities.Play(m_Animator, m_Playable.Playable, m_Graph);
        }
        
        
        public class MyPlayable : PlayableBehaviour
        {
            private Playable m_ActualPlayable;
            private AnimationMixerPlayable m_Mixer;
            protected Playable self { get { return m_ActualPlayable; } }
            protected PlayableGraph graph { get { return self.GetGraph(); } }

            public Playable Playable => self;
            public void SetWeight(float weight) => m_Mixer.SetInputWeight(0, weight);
    
            public override void OnPlayableCreate(Playable playable)
            {
                m_ActualPlayable = playable;

                var mixer = AnimationMixerPlayable.Create(graph, 1, true);
                m_Mixer = mixer;

                self.SetInputCount(1);
                self.SetInputWeight(0, 1);
        
                m_Mixer.SetInputWeight(0, 1);
                graph.Connect(m_Mixer, 0, self, 0);
            }
    
    
            public void Setup(AnimationClip clip)
            {
                clipPlayable = AnimationClipPlayable.Create(graph, clip);
                clipPlayable.SetDuration(clip.length);
        
                var res = graph.Connect(clipPlayable, 0, m_Mixer, 0);
                self.SetInputCount(1);
            }

            public AnimationClipPlayable clipPlayable { get; set; }

            public override void OnGraphStart(Playable playable)
            {
                base.OnGraphStart(playable);
                Debug.Log("START? " + ", " + playable + ", " + (playable.Equals(this.Playable)));
                playable.Play();
            }
        }


    }
}