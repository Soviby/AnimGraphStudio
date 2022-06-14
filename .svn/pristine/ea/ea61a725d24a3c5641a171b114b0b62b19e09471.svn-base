using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace GameCore.Animation
{
    public class AnimLayersNode : AnimNode
    {
        internal AnimLayersNode()
        {
            children = new List<AnimNode>();
            Weight = 1;
        }

        protected override void CreatePlayable(out Playable outPlayable)
        {
            outPlayable = AnimationLayerMixerPlayable.Create(graph.PlayableGraph, 1);
        }

        public void AddLayer()
        {
            var node = new AnimLayerNode();
            AddChild(node);
        }

        void EnsureLayers(int index)
        {
            var maxIndex = children.Count - 1;
            if (maxIndex < index)
            {
                for (var i = 0; i < (index - maxIndex); ++i)
                {
                    AddLayer();
                }
            }
        }

        public AnimLayerNode GetLayer(int index = 0)
        {
            EnsureLayers(index);
            return children[index] as AnimLayerNode;
        }

        internal bool IsLayerAdditive(int index)
        {
            return ((AnimationLayerMixerPlayable)playable).IsLayerAdditive((uint)index);
        }

        internal void SetLayerAdditive(int index, bool value)
        {
            EnsureLayers(index);
            ((AnimationLayerMixerPlayable)playable).SetLayerAdditive((uint)index, value);
        }

        internal void SetLayerMask(int index, AvatarMask mask)
        {
            EnsureLayers(index);
            ((AnimationLayerMixerPlayable)playable).SetLayerMaskFromAvatarMask((uint)index, mask);
        }
    }
}