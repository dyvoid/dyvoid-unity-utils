//c# Example (LookAtPointEditor.cs)
using UnityEngine;
using UnityEditor;

namespace Temple.Utils
{
    [CustomPropertyDrawer(typeof(RangedFloatValue))]
    public class RangedFloatValueDrawer : PropertyDrawer 
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get position
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float lineHeight = 15f;

            // Calculate rects
            var valueRect = new Rect(position.x, position.y, position.width, lineHeight);
            var fromRect = new Rect(position.x, position.y + lineHeight + 1, position.width * 0.2f, lineHeight);
            var rangeRect = new Rect(position.x + position.width * 0.25f, position.y + lineHeight + 1, position.width * 0.5f, lineHeight);
            var toRect = new Rect(position.x + position.width * 0.8f, position.y + lineHeight + 1, position.width * 0.2f, lineHeight);

            SerializedProperty valueProperty = property.FindPropertyRelative("_value");
            SerializedProperty fromProperty = property.FindPropertyRelative("_range").FindPropertyRelative("_from");
            SerializedProperty toProperty = property.FindPropertyRelative("_range").FindPropertyRelative("_to");

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);

            EditorGUI.PropertyField(fromRect, fromProperty, GUIContent.none);
            valueProperty.floatValue = GUI.HorizontalSlider(rangeRect, valueProperty.floatValue, fromProperty.floatValue, toProperty.floatValue);    
            EditorGUI.PropertyField(toRect, toProperty, GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 35.0f;
        }
    }
}