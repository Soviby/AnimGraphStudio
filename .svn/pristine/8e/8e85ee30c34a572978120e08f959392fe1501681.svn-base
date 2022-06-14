using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Animation
{
    public class AnimTest7 : MonoBehaviour
    {
        public AnimComponent animComponent;
        public Object[] motions;
        public bool returnToFirst;
        int index = 0;
        int prevIndex = 0;

        void OnEnable()
        {
            StopAllCoroutines();
            StartCoroutine(PlayCoroutine());
        }

        void ChangeIndex()
        {
            var prev = prevIndex;
            prevIndex = index;
            if (returnToFirst)
            {
                if (index == 0)
                {
                    prev++;
                    index = prev;
                    if (index >= motions.Length)
                        index = 1;
                }
                else
                {
                    index = 0;
                }
            }
            else
            {
                index++;
                if (index >= motions.Length)
                    index = 0;
            }
        }

        IEnumerator PlayCoroutine()
        {
            var graph = animComponent.GetGraph();
            while (true)
            {
                ChangeIndex();
                var state = graph.CrossFade(motions[index]);
                var len = state.Length;
                yield return new WaitForSeconds(len);
            }
        }

        void OnGUI()
        {
            if (returnToFirst)
                GUILayout.Space(20);

            var name = motions[index].name;
            GUILayout.Label($"index: {index}, name: {name}", GUILayout.MinHeight(50));
        }
    }
}
