using EditorUtils.Editor;
using UnityEditor;
using UnityEngine;

namespace EditorUtils.BoldHeader.Editor
{
    [CustomPropertyDrawer(typeof(BoldHeaderAttribute))]
    public class BoldHeaderDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            return EditorGUIUtility.singleLineHeight * 2.1f;
        }

        public override void OnGUI(Rect position)
        {
            var headerAttribute = attribute as BoldHeaderAttribute;
            SashimiEditorGUI.DrawBoldHeader(position, headerAttribute.header);
        }
    }
}