using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace DevTools.Editor
{
    /// <summary>
    ///     Utility class for reusable Editor GUI styles and controls
    /// </summary>
    public static class SashimiEditorGUIUtils
    {
        private static Color boldHeaderColor = new(1f, 0.973f, 0.639f);

        private static GUIStyle boldHeaderStyle;

        private static GUIStyle BoldHeaderStyle
        {
            get
            {
                if (boldHeaderStyle != null)
                {
                    return boldHeaderStyle;
                }

                var style = new GUIStyle(EditorStyles.largeLabel);
                style.normal.textColor = boldHeaderColor;
                boldHeaderStyle = style;
                return style;
            }
        }

        /// <summary>
        ///     Combination of a bold header and a help box.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="name"></param>
        /// <param name="help"></param>
        public static void ComponentDescription(Rect position, string name, string help)
        {
            DrawBoldHeader(position, name);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                help,
                MessageType.Info);
        }

        /// <summary>
        ///     Draws a text header with a horizontal line underneath it.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="text"></param>
        public static void DrawBoldHeader(Rect position, string text)
        {
            position.y += EditorGUIUtility.singleLineHeight * 0.6f;
            GUI.Label(position, text, BoldHeaderStyle);
            position.y += EditorGUIUtility.singleLineHeight * 1.2f;
            NaughtyEditorGUI.HorizontalLine(position, 2, boldHeaderColor);
        }

        public static void DrawBoldHeaderWithLayout(string text)
        {
            float height = EditorGUIUtility.singleLineHeight * 2.8f;
            Rect position = EditorGUILayout.GetControlRect(false, height);

            position.y += EditorGUIUtility.singleLineHeight * 0.6f;
            GUI.Label(position, text, BoldHeaderStyle);
            position.y += EditorGUIUtility.singleLineHeight * 1.2f;
            NaughtyEditorGUI.HorizontalLine(position, 2, boldHeaderColor);
        }
    }
}