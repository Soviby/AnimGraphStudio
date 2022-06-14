using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEditor;
using GameCore.Animation;
using GameCore.Animation.Internal;

using static GameEditor.Animation.AnimEditorUtils;

namespace GameEditor.Animation
{
    public class AnimNodeInspector
    {
        static readonly Color s_TimelineBgColor = new Color(40 / 255f, 40 / 255f, 40 / 255f);
        static readonly Color s_EventColor = new Color(0, 0.8f, 0, 0.8f);
        static readonly Color s_EndEventColor = new Color(0.8f, 0, 0, 0.8f);
        static readonly Color s_NeedleColor = new Color(1, 1, 1, 1);

        static GUIStyle s_TimeLabelStyle;
        static GUIStyle s_EventTipsStyle;
        static string[] s_TimeLabelText = new string[] { "0.0", "0.2", "0.4", "0.6", "0.8", "1.0" };

        static bool s_UseNormalizedTime;
        static bool s_ShowPlayableInfo = true;
        static bool s_ShowBlendData = true;
        static bool s_ShowEvents = true;
        static StringBuilder stringBuilder;

        struct Context
        {
            public AnimNode node;
            public AnimStateNode stateNode;
            public bool isSync;
        }

        static StringBuilder PrepareStringBuilder()
        {
            if (stringBuilder == null)
                stringBuilder = new StringBuilder();
            stringBuilder.Clear();
            return stringBuilder;
        }

        public static void Draw(AnimNode node)
        {
            if (s_TimeLabelStyle == null)
            {
                s_TimeLabelStyle = new GUIStyle(EditorStyles.label);
                s_TimeLabelStyle.alignment = TextAnchor.UpperCenter;

                s_EventTipsStyle = new GUIStyle("AnimationEventTooltip");
                s_EventTipsStyle.alignment = TextAnchor.MiddleLeft;
            }


            var stateNode = node as AnimStateNode;
            var context = new Context
            {
                node = node,
                stateNode = stateNode,
            };
            if (stateNode != null)
            {
                context.isSync = stateNode.IsSync;
                if (context.isSync)
                    EditorGUILayout.HelpBox("This node is synchronised, some properties is controlled by parent", MessageType.Warning);
            }

            EditorGUI.BeginChangeCheck();

            DrawCommon(context);
            DrawLayer(context);
            DrawTime(context);
            DrawSpeed(context);

            if (EditorGUI.EndChangeCheck() && !node.Graph.IsPlaying)
                node.Graph.Evaluate();

            DrawClip(context);
            DrawBlend(context);
            DrawController(context);

            DrawEvents(context);

            if (s_ShowPlayableInfo)
            {
                EditorGUILayout.Space();
                DrawPlayableInfo(context);
            }
        }

        static void DrawPlayableInfo(Context context)
        {
            var node = context.node;
            var p = node.Playable;
            var sb = PrepareStringBuilder();
            sb.AppendLine(InfoString("IsValid", p.IsValid()));
            sb.AppendLine(InfoString("IsDone", p.IsDone()));
            sb.AppendLine(InfoString("InputCount", p.GetInputCount()));
            sb.AppendLine(InfoString("OutputCount", p.GetOutputCount()));
            sb.AppendLine(InfoString("PlayState", p.GetPlayState()));
            sb.AppendLine(InfoString("Speed", p.GetSpeed()));
            sb.AppendLine(InfoString("Duration", p.GetDuration()));
            sb.AppendLine(InfoString("Time", p.GetTime()));

            if (node is AnimLayersNode)
            {
                var almp = (AnimationLayerMixerPlayable)p;
                for (uint i = 0; i < almp.GetInputCount(); ++i)
                    sb.AppendLine(InfoString(string.Format("IsLayerAdditive #{0}", i + 1), almp.IsLayerAdditive(i)));
            }
            else if (node is AnimClipNode)
            {
                var acp = (AnimationClipPlayable)p;
                var clip = acp.GetAnimationClip();
                sb.AppendLine(InfoString("Clip", clip ? clip.name : "(none)"));
                if (clip)
                {
                    sb.AppendLine(InfoString("ClipLength", clip.length));
                }
                sb.AppendLine(InfoString("ApplyFootIK", acp.GetApplyFootIK()));
                sb.AppendLine(InfoString("ApplyPlayableIK", acp.GetApplyPlayableIK()));
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(sb.ToString(), EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        static void DrawCommon(Context context)
        {
            var node = context.node;
            EditorGUI.BeginChangeCheck();
            var weight = EditorGUILayout.Slider("Weight", node.Weight, 0, 1);
            if (EditorGUI.EndChangeCheck())
                node.Weight = weight;
        }

        static void DrawTime(Context context)
        {
            var node = context.stateNode;
            if (node == null)
                return;
            if (context.isSync)
                EditorGUI.BeginDisabledGroup(true);

            var normalizedTime = node.NormalizedTime;
            var times = Mathf.FloorToInt(normalizedTime);
            var percent = normalizedTime - times;
            if (percent == 0 && times > 0)
            {
                percent = 1f;
                times--;
            }
            var maxValue = s_UseNormalizedTime ? 1f : node.Length;
            var value = percent * maxValue;
            var label = s_UseNormalizedTime ? "Normalized Time" : "Time";
            if (node.IsLooping)
                label += $" (x{times})";

            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.Slider(label, value, 0, maxValue);
            if (EditorGUI.EndChangeCheck())
            {
                if (s_UseNormalizedTime)
                    node.NormalizedTime = value;
                else
                    node.Time = value;
            }

            if (context.isSync)
                EditorGUI.EndDisabledGroup();
        }

        static void DrawSpeed(Context context)
        {
            var node = context.node;
            if (context.isSync)
                EditorGUI.BeginDisabledGroup(true);

            EditorGUI.BeginChangeCheck();
            var speed = EditorGUILayout.FloatField("Speed", node.Speed);
            if (EditorGUI.EndChangeCheck())
                node.Speed = speed;

            if (context.isSync)
                EditorGUI.EndDisabledGroup();
        }

        static void DrawBlend(Context context)
        {
            var node = context.node as AnimBlendNode;
            if (node == null)
                return;

            var blend = node.GetContent();
            var contentDirty = false;

            if (blend is AnimBlend1D blend1D)
            {
                var blendNode = node as AnimBlend1DNode;
                EditorGUI.BeginChangeCheck();
                var param = EditorGUILayout.FloatField("Blend Parameter", blendNode.Parameter);
                if (EditorGUI.EndChangeCheck())
                    blendNode.Parameter = param;

                var datas = blend1D.datas;
                if (datas != null && datas.Length > 0)
                {
                    EditorGUILayout.Space();

                    s_ShowBlendData = EditorGUILayout.BeginFoldoutHeaderGroup(s_ShowBlendData, "Blend Space");
                    if (s_ShowBlendData)
                    {
                        var motionWidth = 120f;
                        var syncWidth = 40f;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Motion", GUILayout.Width(motionWidth));
                        GUILayout.Label("Threshold", GUILayout.ExpandWidth(true));
                        GUILayout.Label("Sync", GUILayout.Width(syncWidth));
                        EditorGUILayout.EndHorizontal();

                        for (var i = 0; i < datas.Length; ++i)
                        {
                            GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(20), GUILayout.ExpandWidth(true));
                            var rect = GUILayoutUtility.GetLastRect();
                            var position = rect;
                            var padding = 10f;
                            EditorGUI.BeginDisabledGroup(true);
                            position.width = motionWidth;
                            EditorGUI.ObjectField(position, datas[i].motion, typeof(Object), true);
                            EditorGUI.EndDisabledGroup();

                            EditorGUI.BeginChangeCheck();
                            position.x = rect.x + motionWidth + padding;
                            position.width = rect.width - motionWidth - syncWidth - 2 * padding;
                            datas[i].threshold = EditorGUI.FloatField(position, datas[i].threshold);
                            position.x = rect.x + rect.width - syncWidth;
                            position.width = syncWidth;
                            datas[i].sync = EditorGUI.Toggle(position, datas[i].sync);
                            if (EditorGUI.EndChangeCheck())
                                contentDirty = true;
                        }
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
            }
            else if (blend is AnimBlend2D blend2D)
            {
                var blendNode = node as AnimBlend2DNode;
                EditorGUI.BeginChangeCheck();
                var param = EditorGUILayout.Vector2Field("Blend Parameter", blendNode.Parameter);
                if (EditorGUI.EndChangeCheck())
                    blendNode.Parameter = param;

                EditorGUI.BeginChangeCheck();
                blend2D.blendType = (EAnimBlend2DType)EditorGUILayout.EnumPopup("Blend Type", blend2D.blendType);
                if (EditorGUI.EndChangeCheck())
                    contentDirty = true;

                var datas = blend2D.datas;
                if (datas != null && datas.Length > 0)
                {
                    EditorGUILayout.Space();

                    s_ShowBlendData = EditorGUILayout.BeginFoldoutHeaderGroup(s_ShowBlendData, "Blend Space");
                    if (s_ShowBlendData)
                    {
                        var motionWidth = 120f;
                        var syncWidth = 40f;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Motion", GUILayout.Width(motionWidth));
                        GUILayout.Label("Threshold", GUILayout.ExpandWidth(true));
                        GUILayout.Label("Sync", GUILayout.Width(syncWidth));
                        EditorGUILayout.EndHorizontal();

                        for (var i = 0; i < datas.Length; ++i)
                        {
                            GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(20), GUILayout.ExpandWidth(true));
                            var rect = GUILayoutUtility.GetLastRect();
                            var position = rect;
                            var padding = 10f;
                            EditorGUI.BeginDisabledGroup(true);
                            position.width = motionWidth;
                            EditorGUI.ObjectField(position, datas[i].motion, typeof(Object), true);
                            EditorGUI.EndDisabledGroup();

                            EditorGUI.BeginChangeCheck();
                            position.x = rect.x + motionWidth + padding;
                            position.width = rect.width - motionWidth - syncWidth - 2 * padding;
                            datas[i].threshold = EditorGUI.Vector2Field(position, "", datas[i].threshold);
                            position.x = rect.x + rect.width - syncWidth;
                            position.width = syncWidth;
                            datas[i].sync = EditorGUI.Toggle(position, datas[i].sync);
                            if (EditorGUI.EndChangeCheck())
                                contentDirty = true;
                        }
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
            }

            if (contentDirty)
                AnimStateNodeModifer.UpdateContent(node, blend);
        }

        static void DrawClip(Context context)
        {
            var node = context.node as AnimClipNode;
            if (node == null)
                return;

            var clip = node.GetContent() as AnimationClip;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Clip", clip, node.GetContentType(), false);
            EditorGUI.EndDisabledGroup();
        }

        static void DrawController(Context context)
        {
            var node = context.node as AnimControllerNode;
            if (node == null)
                return;

            var controller = node.GetContent() as RuntimeAnimatorController;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Controller", controller, node.GetContentType(), false);
            EditorGUI.EndDisabledGroup();
        }

        static void DrawLayer(Context context)
        {
            var node = context.node;
            if (node is AnimLayerNode layerNode)
            {
                EditorGUI.BeginChangeCheck();
                var isAdditive = EditorGUILayout.Toggle("Is Additive", layerNode.IsAdditive);
                if (EditorGUI.EndChangeCheck())
                    layerNode.IsAdditive = isAdditive;

                EditorGUI.BeginChangeCheck();
                var mask = EditorGUILayout.ObjectField("Mask", layerNode.Mask, typeof(AvatarMask), false);
                if (EditorGUI.EndChangeCheck())
                    layerNode.Mask = mask as AvatarMask;
            }
        }

        static void DrawEvents(Context context)
        {
            var node = context.stateNode;
            if (node == null)
                return;

            EditorGUILayout.Space();

            s_ShowEvents = EditorGUILayout.BeginFoldoutHeaderGroup(s_ShowEvents, "Events");
            if (!s_ShowEvents)
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
                return;
            }

            GUILayoutUtility.GetRect(GUIContent.none, "box", GUILayout.Height(25), GUILayout.ExpandWidth(true));
            var rect = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawRect(rect, s_TimelineBgColor);

            rect.x += 8f;
            rect.width -= 16f;

            var normalizedTime = node.NormalizedTime;
            var percent = normalizedTime - Mathf.FloorToInt(normalizedTime);
            var segNum = 5;
            var events = node.GetEvents();
            var oldColor = Handles.color;
            {
                var position = new Rect(rect.x, rect.y, rect.width, 10f);
                var p0 = new Vector3(0, position.y, 0);
                var p1 = new Vector3(0, position.y + position.height, 0);
                var inv = 1f / segNum * position.width;
                float x = position.x;
                float textY = position.y + position.height;
                for (var i = 0; i <= segNum; ++i)
                {
                    float tempX = x + i * inv;
                    p0.x = tempX;
                    p1.x = tempX;
                    Handles.DrawAAPolyLine(1f, p0, p1);

                    var textContent = new GUIContent(s_TimeLabelText[i]);
                    var textSize = s_TimeLabelStyle.CalcSize(textContent);
                    var textPos = new Vector2(tempX - textSize.x * 0.5f, textY);
                    GUI.Label(new Rect(textPos, textSize), textContent, s_TimeLabelStyle);
                }
                p1.y = position.y + position.height * 0.5f;
                inv *= 0.5f;
                for (int i = 1; i <= segNum * 2; i += 2)
                {
                    float tempX = x + i * inv;
                    p0.x = tempX;
                    p1.x = tempX;
                    Handles.DrawAAPolyLine(1f, p0, p1);
                }
            }
            GUIContent tipsContent = null;
            Rect tipsRect = default;
            if (events != null && events.Count > 0)
            {
                var position = rect;
                var p = new Vector3(0, position.y + position.height, 0);
                var r = new Rect(0, position.y, 6f, 10f);
                float x = position.x;
                for (var i = 0; i < events.Count; ++i)
                {
                    var e = events[i];
                    float nt = e.normalizedTime;
                    float tempX = x + nt * position.width;
                    p.x = tempX;
                    r.x = tempX - r.width * 0.5f;

                    var ce = Event.current;
                    var hover = r.Contains(ce.mousePosition);
                    Handles.color = s_EventColor;
                    Handles.DrawAAPolyLine(2f, p, new Vector3(p.x, position.y));
                    Handles.DrawAAConvexPolygon(new Vector3(r.xMin, r.yMin), new Vector3(r.xMin, r.yMax), new Vector3(r.xMax, r.yMax), new Vector3(r.xMax, r.yMin));
                    if (hover)
                    {
                        var tipsPos = new Vector2(r.xMax, r.y);
                        var tips = $"Name: {e.eventName}\n" + InfoString("Normalized Time", nt);
                        tipsContent = new GUIContent(tips);
                        var tipsSize = s_EventTipsStyle.CalcSize(tipsContent);
                        tipsRect = new Rect(tipsPos, tipsSize);
                    }
                }
            }
            if (node.OnEnd != null)
            {
                var position = rect;
                var p = new Vector3(position.x + position.width, position.y + position.height, 0);
                var r = new Rect(p.x - 3f, position.y, 6f, 10f);
                Handles.color = s_EndEventColor;
                Handles.DrawAAPolyLine(2f, p, new Vector3(p.x, position.y));
                Handles.DrawAAConvexPolygon(new Vector3(r.xMin, r.yMin), new Vector3(r.xMin, r.yMax), new Vector3(r.xMax, r.yMax), new Vector3(r.xMax, r.yMin));
            }
            {
                var position = rect;
                float x = position.x + percent * position.width;
                Vector3 p0 = new Vector3(x, position.y, 0);
                Vector3 p1 = new Vector3(x, position.y + position.height, 0);
                Handles.color = s_NeedleColor;
                Handles.DrawLine(p0, p1);
            }
            if (tipsContent != null)
            {
                EditorGUI.LabelField(tipsRect, tipsContent, s_EventTipsStyle);
            }

            Handles.color = oldColor;

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}