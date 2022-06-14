using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace GameCore.Animation
{
    public abstract class AnimStateNode : AnimNode
    {
        protected float eventLastTime;
        protected List<AnimEvent> events;
        protected System.Action onEnd;
        protected System.Action<AnimEvent> onEvent;

        internal AnimStateNode prev, next;
        internal int attachCount;

        public abstract float Length { get; }
        public abstract bool IsLooping { get; }
        public abstract bool ApplyFootIK { get; set; }
        public abstract bool ApplyPlayableIK { get; set; }

        public bool IsConstrained => parent is AnimBlendNode;
        protected override bool KeepConnected => IsConstrained;

        public System.Action OnEnd
        {
            get => onEnd;
            set
            {
                onEnd = value;
                updateFlags |= UpdateFlags.Event;
                eventLastTime = NormalizedTime;
            }
        }

        public System.Action<AnimEvent> OnEvent
        {
            get => onEvent;
            set
            {
                onEvent = value;
                updateFlags |= UpdateFlags.Event;
                eventLastTime = NormalizedTime;
            }
        }

        public virtual float Time
        {
            get => (float)playable.GetTime();
            set => playable.SetTime(value);
        }

        public float NormalizedTime
        {
            get
            {
                var length = Length;
                if (length != 0)
                    return Time / length;
                else
                    return 0;
            }
            set => Time = value * Length;
        }

        public bool IsPlaying => IsConnectedInHierarchy;

        public bool IsSync
        {
            get
            {
                var node = GetTopBlend();
                if (node != null)
                {
                    var syncNodes = node.SyncNodes;
                    return syncNodes != null && syncNodes.Contains(this);
                }
                return false;
            }
            set
            {
                var node = GetTopBlend();
                if (node != null)
                {
                    if (value)
                        node.SyncState(this);
                    else
                        node.DesyncState(this);
                }
            }
        }

        public abstract UnityEngine.Object GetContent();
        public abstract System.Type GetContentType();

        public AnimBlendNode GetTopBlend()
        {
            AnimBlendNode node = null;
            for (AnimNode n = this; n != null; n = n.Parent)
            {
                if (n is AnimBlendNode bn)
                    node = bn;
            }
            return node;
        }

        public List<AnimEvent> GetEvents()
        {
            return events;
        }

        public override void Stop()
        {
            base.Stop();
            Time = 0;
            ClearEvents();
        }

        public override void Start()
        {
            base.Start();
            ClearEvents();
        }

        public override void StartFade(float targetWeight, float duration)
        {
            base.StartFade(targetWeight, duration);
            ClearEvents();
        }

        public void ClearEvents()
        {
            onEvent = null;
            onEnd = null;
            events?.Clear();

            updateFlags &= ~UpdateFlags.Event;
            eventLastTime = 0;
        }

        public virtual void AddEvent(float normalziedTime, string eventName, AnimationEvent animationEvent = null)
        {
            if (events == null)
                events = new List<AnimEvent>();

            var ev = new AnimEvent()
            {
                normalizedTime = normalziedTime,
                eventName = eventName,
                animationEvent = animationEvent,
            };

            events.Add(ev);
            events.Sort(AnimUtils.CompareEvent);

            updateFlags |= UpdateFlags.Event;
            eventLastTime = NormalizedTime;
        }

        public override void Destroy()
        {
            base.Destroy();
            ClearEvents();
        }

        internal virtual void UpdateContent(UnityEngine.Object content)
        {
            Debug.Assert(content == GetContent());
        }

        internal override void Update(UpdateFlags flag)
        {
            switch (flag)
            {
                case UpdateFlags.Event:
                    UpdateEvent();
                    return;
            }
            base.Update(flag);
        }

        void UpdateEvent()
        {
            var hasEvents = onEvent != null && events != null && events.Count > 0;
            if (onEnd == null && !hasEvents)
            {
                updateFlags &= ~UpdateFlags.Event;
                return;
            }

            var prevTime = eventLastTime;
            var curTime = NormalizedTime;
            eventLastTime = curTime;
            var forward = curTime > prevTime;

            if (IsLooping)
            {
                var lc = Mathf.FloorToInt(curTime);
                var lp = Mathf.FloorToInt(prevTime);
                var sameLoop = lc == lp;

                if (hasEvents)
                {
                    var tc = curTime - lc;
                    var tp = prevTime - lp;

                    if (forward)
                    {
                        for (var i = 0; i < events.Count; ++i)
                        {
                            var e = events[i];
                            var t = e.normalizedTime;
                            bool pass = sameLoop ? (tp < t && tc >= t) : (tp < t || tc >= t);
                            if (pass)
                            {
                                onEvent.Invoke(e);
                            }
                        }
                    }
                    else
                    {
                        for (var i = events.Count - 1; i >= 0; --i)
                        {
                            var e = events[i];
                            var t = e.normalizedTime;
                            bool pass = sameLoop ? (tp > t && tc <= t) : (tp > t || tc <= t);
                            if (pass)
                            {
                                onEvent.Invoke(e);
                            }
                        }
                    }
                }

                if (onEnd != null)
                {
                    if (!sameLoop)
                        onEnd.Invoke();
                }
            }
            else
            {
                if (hasEvents)
                {
                    var tc = curTime;
                    if (forward)
                    {
                        for (var i = 0; i < events.Count; ++i)
                        {
                            var e = events[i];
                            if (e.handled)
                                continue;
                            if (tc >= e.normalizedTime)
                            {
                                e.handled = true;
                                events[i] = e;
                                onEvent.Invoke(e);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (var i = events.Count - 1; i >= 0; --i)
                        {
                            var e = events[i];
                            if (e.handled)
                                continue;
                            if (tc <= e.normalizedTime)
                            {
                                e.handled = true;
                                events[i] = e;
                                onEvent.Invoke(e);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                if (onEnd != null)
                {
                    if (forward)
                    {
                        if (curTime > 1)
                        {
                            onEnd.Invoke();
                            onEnd = null;
                        }
                    }
                    else
                    {
                        if (curTime < 0)
                        {
                            onEnd.Invoke();
                            onEnd = null;
                        }
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    namespace Internal
    {
        public class AnimStateNodeModifer
        {
            public static void UpdateContent(AnimStateNode node, UnityEngine.Object content)
            {
                node.UpdateContent(content);
            }
        }
    }
#endif 

}