using System;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Temple.Utils
{
    [Serializable]
    public class Range : ISerializationCallbackReceiver
    {
        [SerializeField] private float _from;
        [SerializeField] private float _to;

        public float Size { get; private set; }

        public RangeDirection Direction { get; private set; }

        /**
         * Translates a value within a source range to a target range.
         * The input value is automatically constrained to the source range.
         *
         * @method Translate
         * @static
         * @param value {float} The value that is to be translated
         * @param sourceFrom {float} The first boundary value of the source range
         * @param sourceTo {float} The second boundary value of the source range
         * @param [targetFrom=0] {float} The first boundary value of the target range
         * @param [targetTo=1] {float} The second boundary value of the target range
         * @return {float} The translated value
         */
        public static float Translate(float value, float sourceFrom, float sourceTo, float targetFrom = 0.0f, float targetTo = 1.0f)
        {
            return new Range(sourceFrom, sourceTo).Translate(value, new Range(targetFrom, targetTo));
        }

        /**
	     * Constructs a new Range instance. The from/to values don't have to be in lower/higher order,
	     * since the Range class works with both ascending and descending ranges.
	     *
	     * @constructor
	     * @param from {float} The first boundary value
	     * @param to {float} The second boundary value
	     */
        public Range(float from = 0.0f, float to = 1.0f)
        {
            Set(from, to);
        }

        /**
	     * Set The from/to values of the Range
	     *
         * @method Set
	     * @param from {float} The first boundary value
	     * @param to {float} The second boundary value
	     */
        public void Set(float from, float to)
        {
            _from = from;
            _to = to;

            this.UpdateSize();
            this.UpdateDirection();
        }

        /**
	     * Translates a value within the range to a factor (0-1).
	     * The input value is automatically constrained to the range.
	     *
	     * @method GetFactorFromValue
	     * @param value {float} A value within the range
	     * @return {float} The factor within the range
	     */
        public float GetFactorFromValue(float value)
        {
            return Mathf.Abs((Clamp(value) - _from) / Size);
        }

        /**
	     * Translates a factor (0-1) to a value within the range.
	     * The input value is by default constrained to 0-1.
	     *
	     * @method GetValueFromFactor
	     * @param factor {float} The factor
	     * @return {float} The value within the range
	     */
        public float GetValueFromFactor(float factor)
        {
            return _from + (int) Direction * (factor * Size);
        }

        /**
	     * Translates a value within this range to a target range
	     *
	     * @method Translate
	     * @param value {float} A value within the range
	     * @param targetRange {Range} The target range that the value should be translated to
	     * @return {float} The translated value
	     */
        public float Translate(float value, Range targetRange)
        {
            return targetRange.GetValueFromFactor(GetFactorFromValue(value));
        }

        /**
	     * Clamps a value to the range
	     *
	     * @method Clamp
	     * @param value {float} The value that should be constrained
	     * @return {float} The constrained value
	     */
        public float Clamp(float value)
        {
            if (Direction == RangeDirection.ASCENDING)
            {
                return Mathf.Clamp(value, _from, _to);
            }
            else
            {
                return Mathf.Clamp(value, _to, _from);
            }
        }

        /**
	     * Ping pong a value within the range
	     *
	     * @method PingPong
	     * @param value {float} The value that should be ping ponged
	     * @return {float} The ping ponged value
	     */
        public float PingPong(float value)
        {
            if (Direction == RangeDirection.ASCENDING)
            {
                return PingPong(value, _from, _to);
            }
            else
            {
                return PingPong(value, _to, _from);
            }
        }

        /**
	     * Ping pong a value between an upper and lower limit
	     *
	     * @private
	     * @method PingPong
	     * @param value {float} The value that should be ping ponged
	     * @param lower {float} The lower limit
	     * @param upper {float} The upper limit
	     * @return {float} The ping ponged value
	     */
        private float PingPong(float value, float lower, float upper)
        {
            if (value < lower)
            {
                return lower + ((lower - value) % Size);
            }
            else if (value > upper)
            {
                return upper - ((value - upper) % Size);
            }
            else
            {
                return value;
            }
        }

        /**
	     * Loop a value within the range
	     *
	     * @method Loop
	     * @param value {float} The value that should be looped
	     * @return {float} The looped value
	     */
        public float Loop(float value)
        {
            if (Direction == RangeDirection.ASCENDING)
            {
                return Loop(value, _from, _to);
            }
            else
            {
                return Loop(value, _to, _from);
            }
        }

        /**
	     * Loop a value between an upper and lower limit
	     *
         * @private
	     * @method Loop
	     * @param value {float} The value that should be looped
	     * @param lower {float} The lower limit
	     * @param upper {float} The upper limit
	     * @return {float} The looped value
	     */
        private float Loop(float value, float lower, float upper)
        {
            if (value < lower)
            {
                return upper - ((lower - value) % Size);
            }
            else if (value > upper)
            {
                return lower + ((value - upper) % Size);
            }
            else
            {
                return value;
            }
        }

        /**
	     * Get a random value within the range
	     *
	     * @method GetRandomValue
	     * @return {float} the random number
	     */
        public float GetRandomValue()
        {
            return GetValueFromFactor(Random.value);
        }

        /**
	     * Checks whether a value is within the range
	     *
	     * @method Contains
	     * @param value {float} The value
	     * @return {boolean} Whether the value is within the range
	     */
        public bool Contains(float value)
        {
            return (Direction == RangeDirection.ASCENDING) ? (value >= _from && value <= _to) : (value >= _to && value <= _from);
        }

        /**
	     * Offsets the range by a certain value
	     *
	     * @method Offset
	     * @param offsetValue {float} The value to offset the range with
	     */
        public void Offset(float offsetValue)
        {
            _to += offsetValue;
            _from += offsetValue;
        }

        /**
	     * Scale the range
	     *
	     * @method Scale
	     * @param scaleValue {float} The value to scale the range. May be negative to flip the range.
	     */
        public void Scale(float scaleValue)
        {
            float sizeDelta = (Size - (Size * scaleValue)) * 0.5f;

            if (Direction == RangeDirection.DESCENDING)
            {
                sizeDelta = -sizeDelta;
            }

            _from += sizeDelta;
            _to -= sizeDelta;

            UpdateSize();

            if (scaleValue < 0f)
            {
                UpdateDirection();
            }
        }

        /**
	     * Calculates the size of the range
	     *
	     * @private
	     * @method UpdateSize
	     */
        private void UpdateSize()
        {
            Size = Mathf.Abs(this._from - this._to);
        }

        /**
	     * Updates the direction of the range. 
	     *
	     * @private
	     * @method UpdateDirection
	     */
        private void UpdateDirection()
        {
            Direction = (_to > _from) ? RangeDirection.ASCENDING : RangeDirection.DESCENDING;
        }

        public float From
        {
            get => _from;
            set
            {
                _from = value;

                UpdateSize();
                UpdateDirection();
            }
        }

        public float To
        {
            get => _to;
            set
            {
                _to = value;

                UpdateSize();
                UpdateDirection();
            }
        }

        #region ISerializationCallbackReceiver

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
            UpdateSize();
            UpdateDirection();
        }

        #endregion
    }

    public enum RangeDirection
    {
        DESCENDING = -1,
        ASCENDING = 1,
    }

#if UNITY_EDITOR
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
#endif
}

