//c# Example (LookAtPointEditor.cs)
using UnityEngine;
using UnityEditor;

namespace Temple.Utils
{
    [CustomPropertyDrawer(typeof(Range))]
    public class RangeDrawer : PropertyDrawer 
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get position
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            var fromLabelRect = new Rect(position.x, position.y, position.width * 0.15f, position.height);            
            var fromRect = new Rect(position.x + position.width * 0.15f, position.y, position.width * 0.3f, position.height);
            var toLabelRect = new Rect(position.x + position.width * 0.6f, position.y, position.width * 0.15f, position.height);
            var toRect = new Rect(position.x + position.width * 0.7f, position.y, position.width * 0.3f, position.height);

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.LabelField(fromLabelRect, "From");            
            EditorGUI.PropertyField(fromRect, property.FindPropertyRelative("_from"), GUIContent.none);
            EditorGUI.LabelField(toLabelRect, "To");
            EditorGUI.PropertyField(toRect, property.FindPropertyRelative("_to"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}