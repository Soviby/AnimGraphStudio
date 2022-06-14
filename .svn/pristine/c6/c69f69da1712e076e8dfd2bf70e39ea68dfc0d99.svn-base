using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Animation
{
    [RequireComponent(typeof(AnimComponent))]
    public class AnimURO : MonoBehaviour
    {
        [Range(1, 30)]
        public float updateRate = 10;

        AnimComponent animComponent;
        float lastUpdateTime;
        
        void Awake()
        {
            animComponent = GetComponent<AnimComponent>();
        }

        void OnEnable()
        {
            animComponent.GetGraph().Stop();
            lastUpdateTime = Time.time;
        }

        void OnDisable()
        {
            if (!animComponent)
                return;
            
            animComponent.GetGraph().Start();
        }

        void Update()
        {
            var time = Time.time;
            var deltaTime = time - lastUpdateTime;
            if (deltaTime > 1 / updateRate)
            {
                animComponent.GetGraph().Evaluate(deltaTime);
                lastUpdateTime = time;
            }
        }
    }
}
