using System;
using UnityEngine;

namespace dyvoid.Utility
{
	[Serializable]
	public class ObservableFloat : ObservableValue<float>
	{
		public ObservableFloat(float value = 0.0f) : base(value)
		{
		}
	}
}

#if UNITY_EDITOR
namespace dyvoid.Utility
{
	using UnityEditor;

	[CustomPropertyDrawer(typeof(ObservableFloat))]
	public class ObservableFloatDrawer : PropertyDrawer
	{

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			// Get position
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			// Calculate rects
			var rect = new Rect(position.x, position.y, position.width, position.height);

			// Draw fields - passs GUIContent.none to each so they are drawn without labels
			EditorGUI.PropertyField(rect, property.FindPropertyRelative("value"), GUIContent.none);

			EditorGUI.EndProperty();
		}
	}
}
#endif
