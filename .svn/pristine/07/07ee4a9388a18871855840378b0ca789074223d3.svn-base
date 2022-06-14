using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Animation
{
    public class AnimTest2 : MonoBehaviour
    {
        public AnimComponent animComponent;

        public AnimationClip idleClip;
        public AnimationClip walkClip;
        public AnimationClip runClip;

        public AudioSource leftFootAudioSource;
        public AudioSource rightFootAudioSource;
        public AudioClip footAudioClip;

        int lastMoveStatus;  //0 idle, 1 walk, 2 run

        void OnEnable()
        {
            lastMoveStatus = -1;
        }

        void Update()
        {
            var graph = animComponent.GetGraph();
            var movement = Input.GetAxisRaw("Vertical");
            var run = Input.GetButton("Fire3");

            int moveStatus = 0;
            if (movement != 0)
            {
                moveStatus = run ? 2 : 1;
            }

            var lastStatus = lastMoveStatus;
            lastMoveStatus = moveStatus;
            if (lastStatus == moveStatus)
            {
                return;
            }

            if (moveStatus != 0)
            {
                var clip = run ? runClip : walkClip;
                var otherClip = run ? walkClip : runClip;

                var state = graph.CrossFade(clip);
                state.Speed = movement;

                var otherState = graph.GetState(otherClip);
                if (otherState != null && otherState.IsPlaying)
                    state.NormalizedTime = otherState.NormalizedTime;

                state.AddEvent(0.04f, "Foot0");
                state.AddEvent(0.54f, "Foot1");
                state.OnEvent = e =>
                {
                    var left = e.eventName == "Foot0";
                    var audioSource = left ? leftFootAudioSource : rightFootAudioSource;
                    audioSource.clip = footAudioClip;
                    audioSource.Play();
                };
            }
            else
            {
                graph.CrossFade(idleClip);
            }
        }
    }
}