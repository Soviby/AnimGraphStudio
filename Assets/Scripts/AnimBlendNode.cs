using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace GameCore.Animation
{
    public abstract class AnimBlendNode : AnimStateNode
    {
        public List<AnimStateNode> SyncNodes { get; protected set; }

        public override float Length
        {
            get
            {
                RecalcWeights();

                var length = 0f;
                var totalWeight = 0f;
                if (SyncNodes != null)
                {
                    foreach (var n in SyncNodes)
                    {
                        var w = n.Weight;
                        if (w == 0)
                            continue;
                        var l = n.Length;
                        if (l == 0)
                            continue;
                        length += l * w;
                        totalWeight += w;
                    }
                }

                if (totalWeight > 0)
                    return length / totalWeight;

                foreach (var n in children)
                {
                    var s = n as AnimStateNode;
                    var w = s.Weight;
                    totalWeight += w;
                    length += s.Length * w;
                }

                if (totalWeight <= 0)
                    return 0;
                return length / totalWeight;
            }
        }

        public override float Time
        { 
            get
            {
                RecalcWeights();

                var length = 0f;
                var totalWeight = 0f;
                var weightedNormalizedTime = 0f;

                if (SyncNodes != null)
                {
                    foreach (var n in SyncNodes)
                    {
                        var w = n.Weight;
                        if (w == 0)
                            continue;
                        var l = n.Length;
                        if (l == 0)
                            continue;
                        length += l * w;
                        totalWeight += w;
                        weightedNormalizedTime += n.Time / l * w;
                    }
                }

                if (totalWeight < 0.01f)
                {
                    foreach (var n in children)
                    {
                        var s = n as AnimStateNode;
                        var w = s.Weight;
                        if (w == 0)
                            continue;
                        var l = s.Length;
                        if (l == 0)
                            continue;
                        length += l * w;
                        totalWeight += w;
                        weightedNormalizedTime += s.Time / l * w;
                    }
                }

                if (totalWeight == 0)
                    return base.Time;
                
                totalWeight *= totalWeight;
                return weightedNormalizedTime * length / totalWeight;
            }
            set
            {
                var normalizedValue = value;
                if (value != 0)
                {
                    var l = Length;
                    if (l == 0)
                        normalizedValue = 0;
                    else
                        normalizedValue = value / l;
                }

                if (normalizedValue == 0)
                {
                    foreach (var n in children)
                    {
                        var s = n as AnimStateNode;
                        s.Time = 0;
                    }
                }
                else
                {
                    foreach (var n in children)
                    {
                        var s = n as AnimStateNode;
                        s.NormalizedTime = normalizedValue;
                    }
                }
            }
        }

        public override bool IsLooping
        {
            get
            {
                foreach (var c in children)
                {
                    var s = c as AnimStateNode;
                    if (s.IsLooping)
                        return true;
                }
                return false;
            }
        }

        public override bool ApplyFootIK
        {
            get => throw new System.NotImplementedException();
            set
            {
                foreach (var c in children)
                {
                    var s = c as AnimStateNode;
                    s.ApplyFootIK = value;
                }
            }
        }

        public override bool ApplyPlayableIK
        {
            get => throw new System.NotImplementedException();
            set
            {
                foreach (var c in children)
                {
                    var s = c as AnimStateNode;
                    s.ApplyPlayableIK = value;
                }
            }
        }

        internal AnimBlendNode()
        {
            this.children = new List<AnimNode>();
            updateFlags |= UpdateFlags.BlendParam;
        }

        internal void DesyncState(AnimStateNode state)
        {
            if (SyncNodes == null)
                return;

            SyncNodes.Remove(state);
        }

        internal void SyncState(AnimStateNode state)
        {
            if (state is AnimBlendNode node)
            {
                if (node != this && node.SyncNodes != null)
                {
                    foreach (var n in node.SyncNodes)
                    {
                        SyncState(n);
                    }
                    node.SyncNodes.Clear();
                }
                return;
            }

            if (SyncNodes == null)
            {
                SyncNodes = new List<AnimStateNode>();
                updateFlags |= UpdateFlags.Sync;
            }

            if (SyncNodes.Contains(state))
                return;

            SyncNodes.Add(state);
        }

        void RecalcWeights()
        {
            if ((updateFlags & UpdateFlags.BlendParam) != 0)
            {
                updateFlags &= ~UpdateFlags.BlendParam;
                ForceRecalcWeights();
            }
        }

        internal override void Update(UpdateFlags flag)
        {
            switch (flag)
            {
                case UpdateFlags.BlendParam:
                    RecalcWeights();
                    return;
                case UpdateFlags.Sync:
                    UpdateSync();
                    return;
            }
            base.Update(flag);
        }

        void UpdateSync()
        {
            if (weight == 0)
                return;

            var syncNodes = SyncNodes;
            if (syncNodes.Count == 0)
                return;

            var deltaTime = graph.DeltaTime * RealSpeed;
            if (deltaTime == 0)
                return;

            // assume synced child speed is 1
            var totalWeight = 0f;
            var weightedNormalizedTime = 0f;
            var weightedNormalizedSpeed = 0f;
            foreach (var n in syncNodes)
            {
                var w = n.Weight;
                if (w == 0)
                    continue;
                var l = n.Length;
                if (l == 0)
                    continue;

                totalWeight += w;
                w /= l;
                weightedNormalizedTime += n.Time * w;
                weightedNormalizedSpeed += w;
            }

            // assume each weight is 1
            if (totalWeight < 0.01f)
            {
                foreach (var n in syncNodes)
                {
                    var l = n.Length;
                    if (l == 0)
                        continue;

                    totalWeight += 1;
                    var divLen = 1 / l;
                    weightedNormalizedTime += n.Time * divLen;
                    weightedNormalizedSpeed += divLen;
                }
            }

            weightedNormalizedTime += deltaTime * weightedNormalizedSpeed;
            weightedNormalizedTime /= totalWeight;
            var divDeltaTime = 1f / deltaTime;
            foreach (var n in syncNodes)
            {
                var l = n.Length;
                if (l == 0)
                    continue;

                var normalizedTime = n.Time / l;
                var speed = (weightedNormalizedTime - normalizedTime) * l * divDeltaTime;
                n.Playable.SetSpeed(speed);
            }
        }

        protected override void CreatePlayable(out Playable outPlayable)
        {
            outPlayable = AnimationMixerPlayable.Create(graph.PlayableGraph, children.Count, false);
        }

        protected virtual void ForceRecalcWeights()
        {
        }

        /// <summary>
        /// Includes startIndex
        /// </summary>
        protected void DisableStates(int startIndex, bool forward)
        {
            if (forward)
            {
                for (var i = startIndex; i < children.Count; ++i)
                {
                    children[i].Weight = 0;
                }
            }
            else
            {
                for (var i = startIndex; i >= 0; --i)
                {
                    children[i].Weight = 0;
                }
            }
        }

        protected void NormalizeWeights(float totalWeight)
        {
            if (totalWeight == 1)
                return;

            totalWeight = 1f / totalWeight;
            for (var i = 0; i < children.Count; ++i)
            {
                children[i].Weight *= totalWeight;
            }
        }
    }
}