using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Playables;
using UnityEditor;
using GameCore.Animation;

namespace GameEditor.Animation
{
    public class AnimGraphDrawer
    {
        struct NodeData
        {
            public string label;
            public string text;
            public Color color;
        }

        private static readonly Color s_EdgeColorMin = new Color(1.0f, 1.0f, 1.0f, 0.1f);
        private static readonly Color s_EdgeColorMax = Color.white;
        private static readonly Color s_LegendBackground = new Color(0, 0, 0, 0.1f);

        private static readonly Color s_TimeBarColor = Color.green;
        private static readonly Color s_TimeBarSyncColor = Color.yellow;
        private static readonly Color s_TimeBarBgColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        private static readonly float s_BorderSize = 15;
        private static readonly float s_DefaultMaximumNormalizedNodeSize = 0.8f;
        private static readonly float s_DefaultMaximumNodeSizeInPixels = 100.0f;
        private static readonly float s_DefaultAspectRatio = 1.5f;

        private static readonly int s_NodeMaxFontSize = 14;

        private GUIStyle m_SubTitleStyle;
        private GUIStyle m_NodeRectStyle;

        private static readonly int s_ActiveNodeThickness = 2;
        private static readonly int s_SelectedNodeThickness = 4;
        private static readonly Color s_ActiveNodeColor = Color.white;
        private static readonly Color s_SelectedNodeColor = Color.yellow;

        readonly Dictionary<string, NodeData> nodeDatas = new Dictionary<string, NodeData>();

        System.WeakReference<AnimNode> selectNodeRef = new System.WeakReference<AnimNode>(null);
        AnimNode selectNode
        {
            get
            {
                if (selectNodeRef.TryGetTarget(out var ret))
                {
                    if (ret.IsValid)
                        return ret;
                }
                return null;
            }
            set => selectNodeRef.SetTarget(value);
        }

        float lastUpdateTime = 0;
        float totalUpdateTime = 0;
        float[] dotEasing = new float[1];

        Vector2 inspectorScrollPosition;

        AnimGraphEditorWindow window;

        public AnimGraphDrawer(AnimGraphEditorWindow window)
        {
            this.window = window;

            m_NodeRectStyle = new GUIStyle
            {
                normal =
                {
                    background = (Texture2D) Resources.Load("Node"),
                    textColor = Color.black,
                },
                border = new RectOffset(10, 10, 10, 10),
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                clipping = TextClipping.Clip
            };

            m_SubTitleStyle = EditorStyles.boldLabel;
        }

        public void Reset()
        {
            lastUpdateTime = 0;
            totalUpdateTime = 0;
        }

        public void Draw(Rect area)
        {
            var layout = window.layout;
            var vertices = layout.GetVertices();
            PrepareNodeDatas(vertices);

            var insepctorArea = new Rect(area);
            insepctorArea.width = 300 + s_BorderSize * 2;
            insepctorArea.x = area.xMax - insepctorArea.width;
            DrawInspector(insepctorArea);

            var drawingArea = new Rect(area);
            drawingArea.width -= insepctorArea.width;

            if (selectNode != null)
            {
                var currentEvent = Event.current;
                if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
                {
                    Vector2 mousePos = currentEvent.mousePosition;
                    if (drawingArea.Contains(mousePos))
                    {
                        selectNode = null;
                    }
                }
            }

            DrawGraph(drawingArea, layout);
        }

        private void DrawGraph(Rect area, AnimGraphLayout layout)
        {
            var graph = window.currentGraph;
            var isPlaying = graph.IsPlaying;

            var time = (float)EditorApplication.timeSinceStartup;
            var deltaTime = lastUpdateTime == 0 ? 0 : time - lastUpdateTime;
            if (!isPlaying)
                deltaTime = 0;
            totalUpdateTime += deltaTime;
            lastUpdateTime = time;
            for (var i = 0; i < dotEasing.Length; ++i)
            {
                var t = totalUpdateTime - i * (1f / dotEasing.Length);
                if (t < 0)
                {
                    dotEasing[i] = -1f;
                    continue;
                }
                else
                {
                    t = t - Mathf.FloorToInt(t);
                    var easing = EasingInOut(t, 0, 1);
                    dotEasing[i] = easing;
                }
            }

            var vertices = layout.GetVertices();
            var edges = layout.GetEdges();
            area = new Rect(area.x + s_BorderSize,
                area.y + s_BorderSize,
                area.width - s_BorderSize * 2,
                area.height - s_BorderSize * 2);

            var b = new Bounds(Vector3.zero, Vector3.zero);
            foreach (var v in vertices)
            {
                b.Encapsulate(new Vector3(v.position.x, v.position.y, 0));
            }
            b.Expand(new Vector3(s_DefaultMaximumNormalizedNodeSize, s_DefaultMaximumNormalizedNodeSize, 0));

            var scale = new Vector2(area.width / b.size.x, area.height / b.size.y);
            var offset = new Vector2(-b.min.x, -b.min.y);

            var nodeSize = ComputeNodeSize(scale);

            GUI.BeginGroup(area);

            foreach (var e in edges)
            {
                Vector2 v0 = ScaleVertex(e.source.position, offset, scale);
                Vector2 v1 = ScaleVertex(e.destination.position, offset, scale);
                var node = e.source.node;
                var drawDot = node.IsConnectedInHierarchy && node.Weight > 0f;
                DrawEdge(v1, v0, node.Weight, drawDot);
            }

            var currentEvent = Event.current;

            bool oldSelectionFound = false;
            AnimNode newSelect = null;

            foreach (var v in vertices)
            {
                Vector2 nodeCenter = ScaleVertex(v.position, offset, scale) - nodeSize / 2;
                var nodeRect = new Rect(nodeCenter.x, nodeCenter.y, nodeSize.x, nodeSize.y);

                bool clicked = false;
                if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
                {
                    Vector2 mousePos = currentEvent.mousePosition;
                    if (nodeRect.Contains(mousePos))
                    {
                        clicked = true;
                        currentEvent.Use();
                    }
                }

                bool currentSelection = selectNode == v.node;
                DrawNode(nodeRect, v.node, currentSelection || clicked);

                if (currentSelection)
                {
                    oldSelectionFound = true;
                }
                else if (clicked)
                {
                    newSelect = v.node;
                }
            }

            if (newSelect != null)
            {
                selectNode = newSelect;
            }
            else if (!oldSelectionFound)
            {
                selectNode = null;
            }

            GUI.EndGroup();
        }

        private static float EasingInOut(float t, float a, float b)
        {
            if (t == 0)
                return a;
            if (t == 1)
                return b;
            t *= 2f;
            var d = (b - a) * 0.5f;
            if (t < 1f)
                return d * t * t + a;
            t -= 1f;
            return -d * (t * (t - 2f) - 1f) + a;
        }

        private void DrawNode(Rect nodeRect, AnimNode node, bool selected)
        {
            var typeName = node.GetType().Name;
            var nodeData = nodeDatas[typeName];
            var active = true;

            DrawRect(nodeRect, nodeData.color, nodeData.text, active, selected);
            if (node is AnimStateNode n)
            {
                if (node is AnimBlendNode)
                    return;

                float progress;
                if (n.IsLooping)
                {
                    progress = n.NormalizedTime - Mathf.FloorToInt(n.NormalizedTime);
                }
                else
                {
                    progress = Mathf.Clamp01(n.NormalizedTime);
                }

                DrawProgress(nodeRect, n.IsSync ? s_TimeBarSyncColor : s_TimeBarColor, progress);
            }
        }

        void DrawProgress(Rect rect, Color color, float progress)
        {
            var originalColor = GUI.color;

            const float paddingX = 3f;
            const float paddingY = 6f;
            const float barHeight = 6f;

            if (rect.width > 2 * paddingX && rect.height > 2 * paddingY)
            {
                var h = Mathf.Min(barHeight, rect.height - 2 * paddingY);
                var y = rect.yMax - paddingY - h;
                var w = (rect.width - 2 * paddingX);
                var area = new Rect(rect.x + paddingX, y, w, h);
                GUI.color = s_TimeBarBgColor;
                GUI.DrawTexture(area, Texture2D.whiteTexture);
                area.width *= progress;
                GUI.color = color;
                GUI.DrawTexture(area, Texture2D.whiteTexture);
            }

            GUI.color = originalColor;
        }

        void DrawRect(Rect rect, Color color, string text, bool active, bool selected)
        {
            var originalColor = GUI.color;

            if (selected)
            {
                GUI.color = s_SelectedNodeColor;
                float t = s_SelectedNodeThickness + (active ? s_ActiveNodeThickness : 0.0f);
                GUI.Box(new Rect(rect.x - t, rect.y - t, rect.width + 2 * t, rect.height + 2 * t),
                    string.Empty, m_NodeRectStyle);
            }

            if (active)
            {
                GUI.color = s_ActiveNodeColor;
                GUI.Box(new Rect(rect.x - s_ActiveNodeThickness, rect.y - s_ActiveNodeThickness,
                    rect.width + 2 * s_ActiveNodeThickness, rect.height + 2 * s_ActiveNodeThickness),
                    string.Empty, m_NodeRectStyle);
            }

            GUI.color = color;
            m_NodeRectStyle.fontSize = ComputeFontSize(rect.size, text);
            GUI.Box(rect, text, m_NodeRectStyle);

            GUI.color = originalColor;
        }

        private static int ComputeFontSize(Vector2 nodeSize, string text)
        {
            if (string.IsNullOrEmpty(text))
                return s_NodeMaxFontSize;

            string[] words = text.Split('\n');
            int nbLignes = words.Length;
            int longuestWord = words.Max(s => s.Length);

            int width = longuestWord * (int)(0.8f * s_NodeMaxFontSize);
            int height = nbLignes * (int)(1.5f * s_NodeMaxFontSize);

            float factor = System.Math.Min(nodeSize.x / width, nodeSize.y / height);

            factor = Mathf.Clamp01(factor);

            return Mathf.CeilToInt(s_NodeMaxFontSize * factor);
        }

        private static void GetTangents(Vector2 start, Vector2 end, out Vector3 startTangent, out Vector3 endTangent)
        {
            const float minTangent = 30;
            const float weight = 0.5f;
            float cleverness = Mathf.Clamp01(((start - end).magnitude - 10) / 50);
            startTangent = start + new Vector2((end.x - start.x) * weight + minTangent, 0) * cleverness;
            endTangent = end + new Vector2((end.x - start.x) * -weight - minTangent, 0) * cleverness;
        }

        private void DrawEdge(Vector2 a, Vector2 b, float weight, bool drawDot)
        {
            GetTangents(a, b, out var ta, out var tb);
            var color = Color.Lerp(s_EdgeColorMin, s_EdgeColorMax, weight);
            Handles.DrawBezier(a, b, ta, tb, color, null, 5f);

            if (drawDot)
            {
                var oldColor = Handles.color;
                Handles.color = color;
                foreach (var e in dotEasing)
                {
                    if (e < 0f) continue;
                    var p = GetBezierPoint(a, b, ta, tb, 1f - e);
                    Handles.DrawSolidDisc(p, Vector3.forward, 5f);
                }
                Handles.color = oldColor;
            }
        }

        static Vector3 GetBezierPoint(Vector3 a, Vector3 b, Vector3 ta, Vector3 tb, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * oneMinusT * a +
                3f * oneMinusT * oneMinusT * t * ta +
                3f * oneMinusT * t * t * tb +
                t * t * t * b;
        }

        static Vector2 ScaleVertex(Vector2 v, Vector2 offset, Vector2 scaleFactor)
        {
            return new Vector2((v.x + offset.x) * scaleFactor.x, (v.y + offset.y) * scaleFactor.y);
        }

        static Vector2 ComputeNodeSize(Vector2 scale)
        {
            var extraTickness = (s_SelectedNodeThickness + s_ActiveNodeThickness) * 2.0f;
            var nodeSize = new Vector2(s_DefaultMaximumNormalizedNodeSize * scale.x - extraTickness,
                s_DefaultMaximumNormalizedNodeSize * scale.y - extraTickness);

            float currentAspectRatio = nodeSize.x / nodeSize.y;
            if (currentAspectRatio > s_DefaultAspectRatio)
            {
                nodeSize.x = nodeSize.y * s_DefaultAspectRatio;
            }
            else
            {
                nodeSize.y = nodeSize.x / s_DefaultAspectRatio;
            }

            if (nodeSize.x > s_DefaultMaximumNodeSizeInPixels)
            {
                nodeSize *= s_DefaultMaximumNodeSizeInPixels / nodeSize.x;
            }
            if (nodeSize.y > s_DefaultMaximumNodeSizeInPixels)
            {
                nodeSize *= s_DefaultMaximumNodeSizeInPixels / nodeSize.y;
            }
            return nodeSize;
        }

        private void DrawInspector(Rect area)
        {
            EditorGUI.DrawRect(area, s_LegendBackground);

            area.x += s_BorderSize;
            area.width -= s_BorderSize * 2;
            area.y += s_BorderSize;
            area.height -= s_BorderSize * 2;

            GUILayout.BeginArea(area);
            GUILayout.BeginVertical();

            GUILayout.Label("Inspector", m_SubTitleStyle);
            AnimEditorUtils.LayoutSeparator();

            inspectorScrollPosition = EditorGUILayout.BeginScrollView(inspectorScrollPosition);
            var selectNode = this.selectNode;
            if (selectNode != null)
            {
                AnimNodeInspector.Draw(selectNode);
            }
            else
            {
                AnimGraphInspector.Draw(window.currentGraph);
            }
            EditorGUILayout.EndScrollView();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void PrepareNodeDatas(IEnumerable<AnimGraphLayout.Vertex> vertices)
        {
            nodeDatas.Clear();
            foreach (var v in vertices)
            {
                var node = v.node;
                var nodeType = node.GetType();
                var typeName = nodeType.Name;

                if (nodeDatas.ContainsKey(typeName))
                    continue;

                var label = typeName.EndsWith("Node") ? typeName.Substring(0, typeName.Length - 4) : typeName;
                label = label.StartsWith("Anim") ? label.Substring(4) : label;
                var text = Regex.Replace(label, "([^A-Z])([A-Z])\\B", "$1\n$2");
                var h = (float)System.Math.Abs(label.GetHashCode()) / int.MaxValue;
                nodeDatas[typeName] = new NodeData
                {
                    label = label,
                    text = text,
                    color = Color.HSVToRGB(h, 0.6f, 1.0f),
                };
            }
        }
    }
}