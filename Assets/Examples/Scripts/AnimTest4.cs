using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Animation
{
    public class AnimTest4 : MonoBehaviour
    {
        public AnimComponent animComponent;
        public AnimBlend1D template;

        public bool sync;

        AnimBlend1DNode state;

        void OnEnable()
        {
            var graph = animComponent.GetGraph();
            
            var blend = AnimBlend1D.Create(template);
            blend.datas[1].sync = sync;
            blend.datas[2].sync = sync;
            state = graph.Play(blend) as AnimBlend1DNode;
        }

        public float Speed
        {
            get => state.Parameter;
            set => state.Parameter = value;
        }
       
    }
}