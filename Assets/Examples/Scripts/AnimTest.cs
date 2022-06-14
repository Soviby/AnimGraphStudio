using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Animation
{
    public class AnimTest : MonoBehaviour
    {
        public AnimComponent animComponent;
        public AnimationClip clip;

        public AnimationClip idleClip;

        void OnEnable()
        {
            var graph = animComponent.GetGraph();
            graph.Play(idleClip);
        }

        void OnGUI()
        {
            var graph = animComponent.GetGraph();

            if (GUILayout.Button("Play", GUILayout.MinHeight(50)))
            {
                graph.Play(clip);
            }

            if (GUILayout.Button("Replay", GUILayout.MinHeight(50)))
            {
                var state = graph.Play(clip);
                state.Time = 0;
            }

            if (GUILayout.Button("PlayThenIdle", GUILayout.MinHeight(50)))
            {
                var state = graph.Play(clip);
                state.OnEnd = () => 
                {
                    graph.CrossFade(idleClip);
                };     
            }

            if (GUILayout.Button("CrossFade", GUILayout.MinHeight(50)))
            {
                var state = graph.CrossFade(clip);    
            }
        }
    }
}
