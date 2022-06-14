using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace GameCore.Animation
{
    public class AnimControllerNode : AnimStateNode
    {
        RuntimeAnimatorController controller;

        public AnimatorControllerPlayable ControllerPlayable => (AnimatorControllerPlayable)playable;

        public AnimatorStateInfo StateInfo
        {
            get
            {
                var playable = ControllerPlayable;
                return ControllerPlayable.IsInTransition(0) ?
                    playable.GetNextAnimatorStateInfo(0) :
                    playable.GetCurrentAnimatorStateInfo(0);
            }
        }

        public override float Length => StateInfo.length;
        public override bool IsLooping => StateInfo.loop;

        public override float Time
        { 
            get
            {
                var info = StateInfo;
                return info.normalizedTime * info.length;
            }
            set
            {
                ControllerPlayable.PlayInFixedTime(0, 0, value);
            }
        }

        public override bool ApplyFootIK
        {
            get => false;
            set { }
        }

        public override bool ApplyPlayableIK
        {
            get => false;
            set { }
        }


        internal AnimControllerNode(RuntimeAnimatorController controller)
        {
            this.controller = controller;
        }

        public override void Destroy()
        {
            base.Destroy();
            controller = null;
        }

        public override UnityEngine.Object GetContent()
        {
            return controller;
        }

        public override System.Type GetContentType()
        {
            return typeof(RuntimeAnimatorController);
        }

        protected override void CreatePlayable(out Playable outPlayable)
        {
            outPlayable = AnimatorControllerPlayable.Create(graph.PlayableGraph, controller);
        }
    }
}