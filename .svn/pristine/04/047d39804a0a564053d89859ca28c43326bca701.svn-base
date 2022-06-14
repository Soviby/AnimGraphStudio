using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace GameCore.Animation
{
    public enum EAnimBlend2DType
    {
        Directional,
        Cartesian,
    }

    internal interface IAnimPlayableListener
    {
        void PrepareFrame(Playable playable, ref FrameData info);
    }

    internal static class AnimUtils
    {
        internal static int CompareFloat(float a, float b)
        {
            return a.CompareTo(b);
        }

        internal static int CompareEvent(AnimEvent a, AnimEvent b)
        {
            return a.normalizedTime.CompareTo(b.normalizedTime);
        } 
    }
}