using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Animation
{
    public class AnimBlend2DNode : AnimBlendNode
    {
        AnimBlend2D blend;
        Vector2 parameter;
        Vector2[] cachedVectors;
        float[] cachedMagnitudes;

        public Vector2 Parameter
        {
            get => parameter;
            set
            {
                if (parameter != value)
                {
                    parameter = value;
                    updateFlags |= UpdateFlags.BlendParam;
                }
            }
        }

        internal AnimBlend2DNode(AnimBlend2D blend)
        {
            this.blend = blend;
            blend.Check();

            var datas = blend.datas;
            for (var i = 0; i < datas.Length; ++i)
            {
                var c = new AnimClipNode(datas[i].motion);
                AddChild(c);
                if (datas[i].sync)
                    c.IsSync = true;
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            blend = null;
        }

        public override UnityEngine.Object GetContent()
        {
            return blend;
        }

        public override System.Type GetContentType()
        {
            return typeof(AnimBlend2D);
        }

        void UpdateBlendCache()
        {
            var count = children.Count;
            var datas = blend.datas;
            cachedVectors = new Vector2[count * count];

            switch (blend.blendType)
            {
                case EAnimBlend2DType.Cartesian:
                    {
                        for (var i = 0; i < count; ++i)
                        {
                            var threshold0 = datas[i].threshold;
                            for (var j = i + 1; j < count; ++j)
                            {
                                var threshold1 = datas[j].threshold;
                                var vec = threshold1 - threshold0;
                                vec *= 1f / vec.sqrMagnitude;
                                cachedVectors[i * count + j] = vec;
                                cachedVectors[j * count + i] = -vec;
                            }
                        }
                        break;
                    }
                case EAnimBlend2DType.Directional:
                    {
                        cachedMagnitudes = new float[count];
                        for (var i = 0; i < count; ++i)
                        {
                            cachedMagnitudes[i] = datas[i].threshold.magnitude;
                        }
                        for (var i = 0; i < count; ++i)
                        {
                            var threshold0 = datas[i].threshold;
                            var magnitude0 = cachedMagnitudes[i];
                            for (var j = i + 1; j < count; ++j)
                            {
                                var threshold1 = datas[j].threshold;
                                var magnitude1 = cachedMagnitudes[j];
                                var avg = (magnitude0 + magnitude1) * 0.5f;
                                var vec = new Vector2((magnitude1 - magnitude0) / avg, SignedAngle(threshold0, threshold1));
                                vec *= 1f / vec.sqrMagnitude;
                                cachedVectors[i * count + j] = vec;
                                cachedVectors[j * count + i] = -vec;
                            }
                        }
                        break;
                    }
            }
        }

        protected override void ForceRecalcWeights()
        {
            if (cachedVectors == null)
            {
                UpdateBlendCache();
            }

            var count = children.Count;
            var datas = blend.datas;
            var totalWeight = 0f;

            switch (blend.blendType)
            {
                case EAnimBlend2DType.Cartesian:
                    {
                        for (var i = 0; i < count; ++i)
                        {
                            var node = children[i];
                            var threshold0 = datas[i].threshold;
                            var vec = Parameter - threshold0;

                            var weight = 1f;
                            for (var j = 0; j < count; ++j)
                            {
                                if (i == j)
                                    continue;

                                var w = 1f - Vector2.Dot(vec, cachedVectors[i * count + j]);
                                if (weight > w)
                                    weight = w;
                            }

                            if (weight < 0.01f)
                                weight = 0;

                            node.Weight = weight;
                            totalWeight += weight;
                        }
                        break;
                    }
                case EAnimBlend2DType.Directional:
                    {
                        var magnitude = Parameter.magnitude;
                        for (var i = 0; i < count; ++i)
                        {
                            var node = children[i];
                            var threshold0 = datas[i].threshold;
                            var magnitude0 = cachedMagnitudes[i];
                            var magDiff = magnitude - magnitude0;
                            var ang = SignedAngle(threshold0, Parameter);

                            var weight = 1f;
                            for (var j = 0; j < count; ++j)
                            {
                                if (i == j)
                                    continue;

                                var magnitude1 = cachedMagnitudes[j];
                                var avg = (magnitude0 + magnitude1) * 0.5f;
                                var vec = new Vector2(magDiff / avg, ang);
                                var w = 1f - Vector2.Dot(vec, cachedVectors[i * count + j]);
                                if (weight > w)
                                    weight = w;
                            }

                            if (weight < 0.01f)
                                weight = 0;

                            node.Weight = weight;
                            totalWeight += weight;
                        }
                        break;
                    }
            }

            NormalizeWeights(totalWeight);
        }

        internal override void UpdateContent(UnityEngine.Object content)
        {
            base.UpdateContent(content);
            updateFlags |= UpdateFlags.BlendParam;

            cachedVectors = null;
            cachedMagnitudes = null;

            var datas = blend.datas;
            for (var i = 0; i < datas.Length; ++i)
            {
                (children[i] as AnimStateNode).IsSync = datas[i].sync;
            }
        }

        static float SignedAngle(Vector2 a, Vector2 b)
        {
            if ((a.x == 0 && a.y == 0) || (b.x == 0 && b.y == 0))
            {
                return 0;
            }
            return Mathf.Atan2(a.x * b.y - a.y * b.x, a.x * b.x + a.y * b.y);
        }
    }
}