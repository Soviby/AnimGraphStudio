using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace GameCore.Animation
{
    public class AnimPostProcessorNode : AnimNode, IAnimPlayableListener
    {
        public void PrepareFrame(Playable playable, ref FrameData frameData)
        {
            graph?.HandlePostUpdate(ref frameData);
        }

        protected override void CreatePlayable(out Playable outPlayable)
        {
            var playable = ScriptPlayable<PostProcessorPlayable>.Create(graph.PlayableGraph);
            playable.GetBehaviour().SetListener(this);
            outPlayable = playable;
        }
    }
}