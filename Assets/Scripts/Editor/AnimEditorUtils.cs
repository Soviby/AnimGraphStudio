using System;
using UnityEditor;
using UnityEngine;

namespace GameEditor.Animation
{
    public static class AnimEditorUtils
    {
        public static void LayoutSeparator()
        {
            var rect = EditorGUILayout.GetControlRect(false, 2f);
            rect.height = 1f;
            var oldColor = GUI.color;
            GUI.color = new Color(0.7f, 0.7f, 0.7f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = oldColor;
        }

        public static string InfoString(string key, double value)
        {
            if (Math.Abs(value) < 100000.0)
                return string.Format("{0}: {1:#.###}", key, value);
            if (value == double.MaxValue)
                return string.Format("{0}: +Inf", key);
            if (value == double.MinValue)
                return string.Format("{0}: -Inf", key);
            return string.Format("{0}: {1:E4}", key, value);
        }

        public static string InfoString(string key, int value)
        {
            return string.Format("{0}: {1:D}", key, value);
        }

        public static string InfoString(string key, object value)
        {
            return key + ": " + (value ?? "(none)");
        }
    }
}