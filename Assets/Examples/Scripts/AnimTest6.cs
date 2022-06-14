using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Animation
{
    public class AnimTest6 : MonoBehaviour
    {
        static readonly int MoveParameterID = Animator.StringToHash("Move");

        public AnimComponent animComponent;
        public AnimationClip clip;
        public RuntimeAnimatorController controller;


        void OnEnable()
        {
            var graph = animComponent.GetGraph();
            graph.Play(clip);
        }

        void OnGUI()
        {
            var graph = animComponent.GetGraph();

            if (GUILayout.Button("PlayClip", GUILayout.MinHeight(50)))
            {
                graph.Play(clip);
            }

            if (GUILayout.Button("PlayController", GUILayout.MinHeight(50)))
            {
                graph.Play(controller);
            }

            if (GUILayout.Button("CrossFadeClip", GUILayout.MinHeight(50)))
            {
                graph.CrossFade(clip);
            }

            if (GUILayout.Button("CrossFadeController", GUILayout.MinHeight(50)))
            {
                graph.CrossFade(controller);   
            }
        }
    }
}
