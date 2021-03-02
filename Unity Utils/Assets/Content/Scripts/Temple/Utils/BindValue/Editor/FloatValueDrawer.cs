//c# Example (LookAtPointEditor.cs)
using UnityEngine;
using UnityEditor;

namespace Temple.Utils
{
    [CustomPropertyDrawer(typeof(FloatValue))]
    public class FloatValueDrawer : PropertyDrawer 
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get position
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Calculate rects
            var rect = new Rect(position.x, position.y, position.width, position.height);

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("_value"), GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}