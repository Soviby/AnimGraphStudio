using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Animation
{
    [CreateAssetMenu(menuName = "AnimBlend2D", order = 902)]
    public class AnimBlend2D : ScriptableObject
    {
        [System.Serializable]
        public struct BlendData
        {
            public AnimationClip motion;
            public Vector2 threshold;
            public bool sync;
        }

        public EAnimBlend2DType blendType = EAnimBlend2DType.Directional;
        public BlendData[] datas;

        public void Check()
        {
            var len = datas.Length;
            for (var i = 0; i < len; ++i)
            {
                if (datas[i].motion == null)
                    throw new System.Exception("Empty motion in AnimBlend2D");
            }
        }

        public static AnimBlend2D Create()
        {
            var blend = ScriptableObject.CreateInstance<AnimBlend2D>();
            return blend;
        }

        public static AnimBlend2D Create(AnimBlend2D template)
        {
            var blend = UnityEngine.Object.Instantiate(template);
            return blend;
        }
    }

}