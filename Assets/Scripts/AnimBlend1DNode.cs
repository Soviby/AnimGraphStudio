using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Animation
{
    public class AnimBlend1DNode : AnimBlendNode
    {
        AnimBlend1D blend;
        float parameter;

        public float Parameter
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

        internal AnimBlend1DNode(AnimBlend1D blend)
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
            return typeof(AnimBlend1D);
        }

        protected override void ForceRecalcWeights()
        {
            var datas = blend.datas;
            var count = children.Count;
            if (parameter <= datas[0].threshold)
            {
                children[0].Weight = 1;
                DisableStates(1, true);
            }
            else if (parameter >= datas[count - 1].threshold)
            {
                children[count - 1].Weight = 1;
                DisableStates(count - 2, false);
            }
            else
            {
                var prevThreshold = datas[0].threshold;
                for (var i = 1; i < children.Count; ++i)
                {
                    var threshold = datas[i].threshold;
                    if (parameter > prevThreshold && parameter <= threshold)
                    {
                        var t = (parameter - prevThreshold) / (threshold - prevThreshold);
                        children[i - 1].Weight = 1 - t;
                        children[i].Weight = t;
                        DisableStates(i + 1, true);
                        DisableStates(i - 2, false);
                        break;
                    }
                    prevThreshold = threshold;
                }
            }
        }

        internal override void UpdateContent(UnityEngine.Object content)
        {
            base.UpdateContent(content);
            updateFlags |= UpdateFlags.BlendParam;

            var datas = blend.datas;
            for (var i = 0; i < datas.Length; ++i)
            {
                (children[i] as AnimStateNode).IsSync = datas[i].sync;
            }
        }
    }
}