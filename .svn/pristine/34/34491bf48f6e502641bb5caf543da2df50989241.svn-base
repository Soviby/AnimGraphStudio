using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Animation
{
    [CreateAssetMenu(menuName = "AnimBlend1D", order = 901)]
    public class AnimBlend1D : ScriptableObject
    {
        [System.Serializable]
        public struct BlendData
        {
            public AnimationClip motion;
            public float threshold;
            public bool sync;
        }

        public BlendData[] datas;

        public void Check()
        {
            var len = datas.Length;
            for (var i = 0; i < len; ++i)
            {
                if (datas[i].motion == null)
                    throw new System.Exception("Empty motion in AnimBlend1D");

                if (i == len - 1)
                    continue;
                var curr = datas[i].threshold;
                var next = datas[i + 1].threshold;
                if (next < curr)
                    throw new System.Exception("Invalid thresholds in AnimBlend1D");
            }
        }

        public static AnimBlend1D Create()
        {
            var blend = ScriptableObject.CreateInstance<AnimBlend1D>();
            return blend;
        }

        public static AnimBlend1D Create(AnimBlend1D template)
        {
            var blend = UnityEngine.Object.Instantiate(template);
            return blend;
        }
    }

}