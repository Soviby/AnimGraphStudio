using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Animation
{
    public class AnimTest5 : MonoBehaviour
    {
        public AnimComponent animComponent;
        public AnimationClip runClip;
        public AnimationClip actionClip;
        public AvatarMask mask;

        void OnEnable()
        {
            var graph = animComponent.GetGraph();
            graph.Play(runClip);
            graph.GetLayer(1).Mask = mask;
        }

        void Update()
        {
            var graph = animComponent.GetGraph();
            if (Input.GetMouseButton(0))
            {
                var state = graph.GetLayer(1).CrossFade(actionClip);
                state.OnEnd = () => graph.GetLayer(1).StartFade(0, 0.25f);
            }
        }
       
    }
}