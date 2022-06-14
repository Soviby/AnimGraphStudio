using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Animation
{
    public class AnimTest8 : MonoBehaviour
    {
        public AnimComponent animComponent0;
        public AnimComponent animComponent1;
        public AnimationClip clip;

        public float lodDistance = 5;

        void OnEnable()
        {
            animComponent0.GetGraph().Play(clip);
            animComponent1.GetGraph().Play(clip);
        }

        void Update()
        {
            var trans = animComponent1.transform;
            var delta = Camera.main.transform.position - trans.position;
            var dist2 = delta.sqrMagnitude;
            animComponent1.GetComponent<AnimURO>().enabled = dist2 > lodDistance * lodDistance;
        }
    }
}
