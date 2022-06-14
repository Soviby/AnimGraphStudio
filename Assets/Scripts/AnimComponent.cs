using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace GameCore.Animation
{
    [RequireComponent(typeof(Animator))]
    public class AnimComponent : MonoBehaviour
    {
        AnimGraph graph;
        bool initFlag;

#if UNITY_EDITOR
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        string GraphEditorName
        {
            get => graph != null ? graph.EditorName : null;
            set
            {
                if (graph != null)
                    graph.EditorName = value;
            }
        }
#endif

        protected virtual void OnDestroy()
        {
            if (!IsGraphValid())
                return;

            graph.Destroy();
            graph = null;
            initFlag = true;
        }

        public bool IsGraphValid()
        {
            return graph != null && graph.IsValid;
        }

        public AnimGraph GetGraph()
        {
            InitGraph();
            return graph;
        }

        void InitGraph()
        {
            if (initFlag)
                return;

            var animator = GetComponent<Animator>();
            graph = new AnimGraph(animator);
            initFlag = true;
        }
    }
}
