using UnityEngine;
using UnityEngine.Playables;

namespace GameCore.Animation
{
    internal class RootPlayable : PlayableBehaviour
    {
        IAnimPlayableListener listener;

        internal void SetListener(IAnimPlayableListener listener)
        {
            this.listener = listener;
        }

        // public override void OnPlayableCreate(Playable playable)
        // {
        // }

        // public override void OnPlayableDestroy(Playable playable)
        // {
        // }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (listener != null)
                listener.PrepareFrame(playable, ref info);
        }
    }

    internal class PostProcessorPlayable : PlayableBehaviour
    {
        IAnimPlayableListener listener;

        internal void SetListener(IAnimPlayableListener listener)
        {
            this.listener = listener;
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (listener != null)
                listener.PrepareFrame(playable, ref info);
        }
    }
}
