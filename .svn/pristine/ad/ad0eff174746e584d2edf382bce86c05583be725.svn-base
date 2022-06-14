using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace GameCore.Animation
{
    public class AnimLayerNode : AnimNode
    {
        AvatarMask mask;

        public AvatarMask Mask
        {
            get => mask;
            set
            {
                mask = value;
                if (parent is AnimLayersNode layers)
                    layers.SetLayerMask(Index, value);
            }
        }

        public bool IsAdditive
        {
            get
            {
                if (parent is AnimLayersNode layers)
                    return layers.IsLayerAdditive(Index);
                return false;
            }
            set
            {
                if (parent is AnimLayersNode layers)
                    layers.SetLayerAdditive(Index, value);
            }
        }

        public AnimStateNode CurrentState { private set; get; }

        internal AnimLayerNode()
        {
            children = new List<AnimNode>();
        }

        public AnimStateNode Play(AnimStateNode node)
        {
            Weight = 1;

            AddChild(node);
            node.Start();
            for (var i = children.Count - 1; i >= 0; --i)
            {
                var n = children[i];
                if (n != node)
                {
                    n.Stop();
                }
            }

            CurrentState = node;
            return node;
        }

        public AnimStateNode CrossFade(AnimStateNode node, float duration = 0.25f)
        {
            if (weight == 0 || duration <= 0)
            {
                return Play(node);
            }

            duration *= Mathf.Abs(1 - node.Weight);

            StartFade(1, duration);

            AddChild(node);
            node.StartFade(1, duration);
            for (var i = children.Count - 1; i >= 0; --i)
            {
                var n = children[i];
                if (n != node)
                {
                    n.StartFade(0, duration);
                }
            }

            CurrentState = node;
            return node;
        }

        public AnimStateNode Play(Object content)
        {
            var graph = Graph;
            if (graph == null)
                return null;
            return Play(graph.GetOrCreateState(content));
        }

        public AnimStateNode CrossFade(Object content, float duration = 0.25f)
        {
            var graph = Graph;
            if (graph == null)
                return null;
            return CrossFade(graph.GetOrCreateState(content));
        }

        public override void Stop()
        {
            base.Stop();
            CurrentState = null;
            for (var i = children.Count - 1; i >= 0; --i)
            {
                var n = children[i];
                n.Stop();
            }
        }

        protected override void CreatePlayable(out Playable outPlayable)
        {
            outPlayable = AnimationMixerPlayable.Create(graph.PlayableGraph);
        }
    }
}