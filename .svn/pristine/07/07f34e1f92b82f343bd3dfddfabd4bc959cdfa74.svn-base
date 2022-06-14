using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace GameCore.Animation
{
    public abstract class AnimNode
    {
        [System.Flags]
        internal enum UpdateFlags
        {
            Fade = 1,
            BlendParam = 2,
            Weight = 4,
            Sync = 8,
            Event = 16,
        }

        protected AnimNode() { }

        protected Playable playable;
        protected AnimNode parent;
        protected List<AnimNode> children;
        protected AnimGraph graph;
        protected float weight;
        protected float speed = 1;
        protected float fadeTargetWeight;
        protected float fadeSpeed;
        internal UpdateFlags updateFlags;

        public Playable Playable => playable;
        public bool IsValid => playable.IsValid();
        public AnimGraph Graph => graph;
        public AnimNode Parent => parent;
        public List<AnimNode> Children => children;

        public float Weight
        {
            get => weight;
            set => SetWeight(value);
        }

        public float Speed
        {
            get => speed;
            set
            {
                speed = value;
                playable.SetSpeed(value);
            }
        }

        float ParentRealSpeed
        {
            get
            {
                var speed = 1f;
                for (var p = parent; p != null; p = p.parent)
                {
                    speed *= p.speed;
                }
                return speed;
            }
        }

        public float RealSpeed
        {
            get => speed * ParentRealSpeed;
            set => speed = value / ParentRealSpeed;
        }

        public int Index { get; internal set; }

        public float FadeSpeed => fadeSpeed;

        public float FadeTargetWeight => fadeTargetWeight;

        public virtual bool IsConnected
        {
            get
            {
                if (parent == null)
                    return false;
                var parentPlayable = parent.playable;
                if (!parentPlayable.GetInput(Index).IsValid())
                    return false;
                return true;
            }
        }

        public bool IsConnectedInHierarchy
        {
            get
            {
                if (!IsConnected)
                    return false;
                if (parent != null)
                    return parent.IsConnectedInHierarchy;
                return true;
            }
        }

        protected virtual bool KeepConnected => true;

        public int Depth
        {
            get
            {
                var depth = 1;
                for (var p = parent; p != null; p = p.parent)
                {
                    depth++;
                }
                return depth;
            }
        }

        public void SetWeight(float value, bool keepFade = false)
        {
            if (weight != value)
            {
                weight = value;
                updateFlags |= UpdateFlags.Weight;
            }

            if (!keepFade)
            {
                fadeTargetWeight = value;
                fadeSpeed = 0;
            }
        }

        public virtual void Stop()
        {
            SetWeight(0, false);
        }

        public virtual void Start()
        {
            SetWeight(1, false);
        }

        public virtual void StartFade(float targetWeight, float duration)
        {
            if (duration <= 0 || targetWeight == weight)
            {
                if (targetWeight == 0)
                    Stop();
                else
                    SetWeight(targetWeight, false);
            }
            else
            {
                fadeTargetWeight = targetWeight;
                fadeSpeed = Mathf.Abs(weight - targetWeight) / duration;
                updateFlags |= UpdateFlags.Fade;
            }
        }

        public virtual void Destroy()
        {
            if (children != null)
            {
                for (var i = children.Count - 1; i >= 0; --i)
                {
                    var n = children[i];
                    n.Destroy();
                }
            }

            DestroyPlayable();
            SetParent(null);
        }

        protected virtual void UpdateWeight()
        {
            updateFlags &= ~UpdateFlags.Weight;

            if (parent == null)
                return;

            if (!KeepConnected)
            {
                if (weight == 0)
                {
                    DisconnectPlayable();
                    return;
                }

                if (!IsConnected)
                {
                    ConnectPlayable();

                    if (this is AnimStateNode n)
                    {
                        n.attachCount++;
                        Graph.AttachState(n);
                    }
                }
            }

            parent.playable.SetInputWeight(Index, weight);
        }

        protected virtual void UpdateFade()
        {
            if (fadeSpeed == 0)
            {
                updateFlags &= ~UpdateFlags.Fade;
                return;
            }

            var step = fadeSpeed * ParentRealSpeed * graph.DeltaTime;
            if (step < 0)
                step = -step;

            var delta = fadeTargetWeight - weight;
            if (delta > 0)
            {
                if (delta > step)
                {
                    SetWeight(weight + step, keepFade: true);
                    return;
                }
            }
            else
            {
                if (-delta > step)
                {
                    SetWeight(weight - step, true);
                    return;
                }
            }

            if (fadeTargetWeight == 0)
                Stop();
            else
                SetWeight(fadeTargetWeight, false);
        }

        protected abstract void CreatePlayable(out Playable outPlayable);

        internal virtual void Update(UpdateFlags flag)
        {
            switch (flag)
            {
                case UpdateFlags.Fade:
                    UpdateFade();
                    break;
                case UpdateFlags.Weight:
                    UpdateWeight();
                    break;
            }
        }

        internal void ConnectPlayable()
        {
            if (parent == null)
                return;

            if (weight == 0 && !KeepConnected)
                return;

            var parentPlayable = parent.playable;
            if (!parentPlayable.IsValid())
                return;

            graph.PlayableGraph.Connect(playable, 0, parentPlayable, Index);
            parentPlayable.SetInputWeight(Index, weight);
        }

        internal void DisconnectPlayable()
        {
            if (parent == null)
                return;

            var parentPlayable = parent.playable;
            if (!parentPlayable.IsValid())
                return;

            if (IsConnected)
                graph.PlayableGraph.Disconnect(parentPlayable, Index);
        }

        internal void CreatePlayable()
        {
            CreatePlayable(out playable);

            if (speed != 1)
                playable.SetSpeed(speed);

            if (parent != null)
                ConnectPlayable();
        }

        internal void DestroyPlayable()
        {
            if (playable.IsValid())
                graph.PlayableGraph.DestroyPlayable(playable);
        }

        protected void AddChildToList(AnimNode node)
        {
            if (children != null)
            {
                var index = children.Count;
                node.Index = index;
                children.Add(node);

                if (playable.IsValid())
                    playable.SetInputCount(children.Count);
            }

            node.ConnectPlayable();
        }

        protected void RemoveChildFromList(AnimNode node)
        {
            node.DisconnectPlayable();

            if (children != null)
            {
                var index = node.Index;
                var lastIndex = children.Count - 1;
                if (index < lastIndex)
                {
                    var temp = children[lastIndex];
                    temp.DisconnectPlayable();
                    temp.Index = index;
                    children[index] = temp;
                    temp.ConnectPlayable();
                }
                node.Index = 0;
                children.RemoveAt(lastIndex);

                if (playable.IsValid())
                    playable.SetInputCount(lastIndex);
            }
        }

        internal void SetGraph(AnimGraph newGraph)
        {
            if (graph == newGraph)
                return;

            if (graph != null)
            {
                graph.UnregisterState(this as AnimStateNode);
                DestroyPlayable();
            }

            graph = newGraph;

            if (graph != null)
            {
                CreatePlayable();
                graph.RegisterState(this as AnimStateNode);
            }

            if (children != null)
            {
                for (var i = children.Count - 1; i >= 0; --i)
                {
                    children[i].SetGraph(graph);
                }
            }
        }

        internal void AddChild(AnimNode node)
        {
            node.SetParent(this);
        }

        internal void SetParent(AnimNode newParent)
        {
            if (parent == newParent)
                return;

            if (parent != null)
            {
                parent.RemoveChildFromList(this);
            }

            SetGraph(newParent?.graph);
            parent = newParent;

            if (parent != null)
            {
                parent.AddChildToList(this);
            }
        }
    }
}