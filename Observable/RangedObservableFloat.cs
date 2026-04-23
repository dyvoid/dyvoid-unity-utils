using System;
using UnityEditor;
using UnityEngine;


namespace Utility
{
	[Serializable]
	public class RangedObservableFloat : ObservableFloat
	{
		[SerializeField] private ValueRange range = new ValueRange();

		public ValueRange Range => range;

		private RangedObservableFloat leader;


		public RangedObservableFloat(float value = 0.0f, float rangeFrom = 0.0f, float rangeTo = 1.0f) : base(value)
		{
			this.SetRange(rangeFrom, rangeTo);
		}

		public void SetRange(float from, float to)
		{
			range.Set(from, to);
			// Yes really. Range/modifiers may cause value to change, so we trigger
			// the range/modifiers by using the value setter.
			Value = Value;
		}

		public override float Value
		{
			set { base.Value = Range.Clamp(value); }
		}

		private void OnLeaderValueChanged(float newValue, float oldValue)
		{
			Value = leader.Range.Remap(newValue, Range);
		}

		public void ClearBounds()
		{
			range.Set(float.MinValue, float.MaxValue);
		}

		public void Follow(RangedObservableFloat leader, bool getInitialValue = true)
		{
			if (this.leader != null)
			{
				this.leader.Unsubscribe(this.OnLeaderValueChanged);
			}

			this.leader = leader;
			this.leader.Subscribe(this.OnLeaderValueChanged, getInitialValue);
		}

		public void Unfollow()
		{
			if (leader != null)
			{
				leader.Unsubscribe(this.OnLeaderValueChanged);
				leader = null;
			}
		}

		#region ISerializationCallbackReceiver

		public override void OnAfterDeserialize()
		{
			base.OnAfterDeserialize();
			value = range.Clamp(value);
		}

		#endregion


	}
}

#if UNITY_EDITOR
namespace Utility
{
	using UnityEngine;
    using Range = Utility.ValueRange;

    [CustomPropertyDrawer(typeof(RangedObservableFloat))]
    public class RangedObservableFloatDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get position
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float lineHeight = 17f;

            // Calculate rects
            var valueRect = new Rect(position.x, position.y, position.width, lineHeight);
            var fromRect = new Rect(position.x, position.y + lineHeight + 2, position.width * 0.2f, lineHeight);
            var rangeRect = new Rect(position.x + position.width * 0.25f, position.y + lineHeight + 2, position.width * 0.5f, lineHeight);
            var toRect = new Rect(position.x + position.width * 0.8f, position.y + lineHeight + 2, position.width * 0.2f, lineHeight);

            SerializedProperty valueProperty = property.FindPropertyRelative("_value");
            SerializedProperty fromProperty = property.FindPropertyRelative("_range").FindPropertyRelative("From");
            SerializedProperty toProperty = property.FindPropertyRelative("_range").FindPropertyRelative("To");

            // Draw fields - pass GUIContent.none to each so they are drawn without labels
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
            return 36.0f;
        }
    }
}
#endif
