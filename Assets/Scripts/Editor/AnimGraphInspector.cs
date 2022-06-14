using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using UnityEditor;
using GameCore.Animation;
using GameCore.Animation.Internal;

using static GameEditor.Animation.AnimEditorUtils;

namespace GameEditor.Animation
{
    public class AnimGraphInspector
    {
        static StringBuilder stringBuilder;

        static float s_GraphStepInterval = 0.033f;

        static StringBuilder PrepareStringBuilder()
        {
            if (stringBuilder == null)
                stringBuilder = new StringBuilder();
            stringBuilder.Clear();
            return stringBuilder;
        }

        public static void Draw(AnimGraph graph)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Animator", graph.Animator, typeof(Animator), true);
            EditorGUI.EndDisabledGroup();

            graph.IsPlaying = EditorGUILayout.Toggle("Is Playing", graph.IsPlaying);

            if (!graph.IsPlaying)
            {
                s_GraphStepInterval = EditorGUILayout.FloatField("Step Interval", s_GraphStepInterval);
                if (GUILayout.Button("Step"))
                {
                    graph.Evaluate(s_GraphStepInterval);
                }
            }

            EditorGUILayout.Space();
            DrawDebugInfo(graph);
        }

        static void DrawDebugInfo(AnimGraph graph)
        {
            var info = AnimGraphDebugInfo.FromGraph(graph);

            var sb = PrepareStringBuilder();
            sb.AppendLine(InfoString("IsValid", graph.IsValid));
            sb.AppendLine(InfoString("StateCount", info.stateCount));
            sb.AppendLine(InfoString("YoungCount", info.youngCount));
            sb.AppendLine(InfoString("OldCount", info.oldCount));

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(sb.ToString(), EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }
    }

}