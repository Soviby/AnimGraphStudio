using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace GameCore.Animation
{
    public class AnimClipNode : AnimStateNode
    {
        AnimationClip clip;

        public override float Length => clip.length;
        public override bool IsLooping => clip.isLooping;

        public override bool ApplyFootIK
        {
            get => ((AnimationClipPlayable)playable).GetApplyFootIK();
            set => ((AnimationClipPlayable)playable).SetApplyFootIK(value);
        }

        public override bool ApplyPlayableIK
        {
            get => ((AnimationClipPlayable)playable).GetApplyPlayableIK();
            set => ((AnimationClipPlayable)playable).SetApplyPlayableIK(value);
        }

        public override void AddEvent(float normalziedTime, string eventName, AnimationEvent animationEvent = null)
        {
            if (events == null || events.Count == 0)
            {
                AddAnimationEvents();
            }
            base.AddEvent(normalziedTime, eventName, animationEvent);
        }

        void AddAnimationEvents()
        {
            if (events == null)
                events = new List<AnimEvent>();

            var clipEvents = clip.events;
            var clipLength = clip.length;
            for (var i = 0; i < clipEvents.Length; ++i)
            {
                var clipEvent = clipEvents[i];
                var ev = new AnimEvent()
                {
                    normalizedTime = clipEvent.time / clipLength,
                    eventName = clipEvent.functionName,
                    animationEvent = clipEvent,
                };
                events.Add(ev);
            }
        }

        internal AnimClipNode(AnimationClip clip)
        {
            this.clip = clip;
        }

        public override void Destroy()
        {
            base.Destroy();
            clip = null;
        }

        public override UnityEngine.Object GetContent()
        {
            return clip;
        }

        public override System.Type GetContentType()
        {
            return typeof(AnimationClip);
        }

        protected override void CreatePlayable(out Playable outPlayable)
        {
            outPlayable = AnimationClipPlayable.Create(graph.PlayableGraph, clip);
        }
    }
}