using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEditor;
using GameCore.Animation;
using GameCore.Animation.Internal;

namespace GameEditor.Animation
{
    public class AnimGraphEditorWindow : EditorWindow, IHasCustomMenu
    {
        static readonly float s_ToolbarHeight = 19f;
        internal AnimGraphLayout layout;
        internal AnimGraphDrawer drawer;

        System.WeakReference<AnimGraph> currentGraphRef = new System.WeakReference<AnimGraph>(null);
        internal AnimGraph currentGraph
        {
            get
            {
                if (currentGraphRef.TryGetTarget(out var ret))
                    return ret;
                return null;
            }
            set => currentGraphRef.SetTarget(value);
        }

        private static void ShowMessage(string msg)
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label(msg);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void Reset()
        {
            layout?.Reset();
            drawer?.Reset();
        }

        private AnimGraph GetSelectedGraphInToolBar(List<AnimGraph> graphs, AnimGraph currentGraph)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Width(position.width));

            List<string> options = new List<string>(graphs.Count);
            foreach (var graph in graphs)
            {
                var name = graph.EditorName;
                name = !string.IsNullOrEmpty(name) ? name : "[Unnamed]";
                while (options.Contains(name))
                {
                    name += "#";
                }
                options.Add(name);
            }

            int currentSelection = graphs.IndexOf(currentGraph);
            int newSelection = EditorGUILayout.Popup(currentSelection != -1 ? currentSelection : 0, options.ToArray(), GUILayout.Width(200));

            AnimGraph selectedGraph = null;
            if (newSelection != -1)
                selectedGraph = graphs[newSelection];

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            return selectedGraph;
        }

        void OnGUI()
        {
            var graphs = AnimGraphCollector.graphs;
            if (graphs.Count == 0)
            {
                ShowMessage("No graph in the scene");
                Reset();
                return;
            }

            GUILayout.BeginVertical();
            var graph = GetSelectedGraphInToolBar(graphs, currentGraph);
            GUILayout.EndVertical();

            if (graph != currentGraph)
            {
                currentGraph = graph;
                Reset();
            }

            if (!currentGraph.IsValid)
            {
                ShowMessage("Selected graph is invalid");
                Reset();
                return;
            }

            if (layout == null)
            {
                layout = new AnimGraphLayout();
                drawer = new AnimGraphDrawer(this);
            }

            layout.CalcLayout(currentGraph);
            var area = new Rect(0, s_ToolbarHeight, position.width, position.height - s_ToolbarHeight);
            drawer.Draw(area);
        }

        void Update()
        {
            if (EditorApplication.isPlaying)
                Repaint();
        }

        void OnInspectorUpdate()
        {
            if (!EditorApplication.isPlaying)
                Repaint();
        }

        void OnDisable()
        {
            Reset();
        }

        [MenuItem("Window/Anim Graph Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<AnimGraphEditorWindow>("Anim Graph");
            window.minSize = new Vector2(500, 200);
        }

        public virtual void AddItemsToMenu(GenericMenu menu)
        {
            //menu.AddItem(new GUIContent("Legend"), m_GraphSettings.showLegend, ToggleLegend);
        }

    }
}
