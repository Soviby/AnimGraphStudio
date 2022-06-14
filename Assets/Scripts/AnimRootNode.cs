using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace GameCore.Animation
{
    public class AnimRootNode : AnimNode, IAnimPlayableListener
    {
        public override bool IsConnected => true;

        internal AnimRootNode(AnimGraph graph)
        {
            children = new List<AnimNode>();
            SetGraph(graph);

            AddChild(new AnimLayersNode());
            AddChild(new AnimPostProcessorNode());
        }

        public AnimLayersNode GetLayers()
        {
            return children[0] as AnimLayersNode;
        }

        public AnimPostProcessorNode GetPostProcessor()
        {
            return children[1] as AnimPostProcessorNode;
        }

        public void PrepareFrame(Playable playable, ref FrameData frameData)
        {
            graph?.HandlePreUpdate(ref frameData);
        }

        protected override void CreatePlayable(out Playable outPlayable)
        {
            var playable = ScriptPlayable<RootPlayable>.Create(graph.PlayableGraph, 2);
            playable.GetBehaviour().SetListener(this);
            outPlayable = playable;
        }
    }
}