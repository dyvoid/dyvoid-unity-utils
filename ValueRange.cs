using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace dyvoid.Utility
{
	/// <summary>
	/// Defines a range with two boundary values. 
	/// </summary>
	/// <remarks>
	/// The boundary values do not have to be in lower/higher order,
	/// since the range works with both ascending and descending values.
	/// </remarks>
	[Serializable]
	public struct ValueRange : IEquatable<ValueRange>, IFormattable
	{
		/// <summary>
		/// The start boundary of the range.
		/// </summary>
		/// <remarks>
		/// The boundary values do not have to be in lower/higher order,
		/// since the range works with both ascending and descending values.
		/// </remarks>
		public float From;

		/// <summary>
		/// The end boundary of the range.
		/// </summary>
		/// <remarks>
		/// The boundary values do not have to be in lower/higher order,
		/// since the range works with both ascending and descending values.
		/// </remarks>
		public float To;

		/// <summary>
		/// The size of the range
		/// </summary>
		public float Size => Mathf.Abs(From - To);

		/// <summary>
		/// The direction of the range (ascending or descending)
		/// </summary>
		public RangeDirection Direction => (To >= From) ? RangeDirection.Ascending : RangeDirection.Descending;


		/// <summary>
		/// The lower boundary of the range.
		/// </summary>
		/// <remarks>
		/// This value is inferred from the From/To values and setting it will directly
		/// modify the From/To values as well.
		/// </remarks>
		public float LowerBoundary
		{
			get => (To >= From) ? From : To;
			set
			{
				if (To >= From)
				{
					From = value;
				}
				else
				{
					To = value;
				}
			}
		}

		/// <summary>
		/// The upper boundary of the range.
		/// </summary>
		/// <remarks>
		/// This value is inferred from the From/To values and setting it will directly
		/// modify the From/To values as well.
		/// </remarks>
		public float UpperBoundary
		{
			get => (To >= From) ? To : From;
			set
			{
				if (To >= From)
				{
					To = value;
				}
				else
				{
					From = value;
				}
			}
		}

		///	<summary>
		/// Constructs a new Range instance.
		/// </summary>
		/// <remarks>
		/// The from/to values don't have to be in lower/higher order,
		/// since the range works with both ascending and descending values.
		/// </remarks>
		/// <param name="from">The start boundary value</param>
		/// <param name="to">The end boundary value</param>
		public ValueRange(float from = 0.0f, float to = 1.0f)
		{
			From = from;
			To = to;
		}

		/// <summary>
		/// Set the from/to values of the range
		/// </summary>
		/// <remarks>
		/// The from/to values don't have to be in lower/higher order,
		/// since the range works with both ascending and descending values.
		/// </remarks>
		/// <param name="from">The start boundary value</param>
		/// <param name="to">The end boundary value</param>
		public ValueRange Set(float from, float to)
		{
			From = from;
			To = to;

			return this;
		}

		/// <summary>
		/// Linearly interpolates between the values of the range by a percentage (0-1).
		/// </summary>
		/// <remarks>
		/// The input value is clamped to 0-1 by default.
		/// </remarks>
		/// <param name="percentage">The percentage</param>
		/// <returns>The value within the range</returns>
		public float GetValueAt(float percentage)
		{
			return Mathf.Lerp(From, To, percentage);
		}

		/// <summary>
		/// Determines where a value within the range falls.
		/// </summary>
		/// <remarks>
		/// The input value is automatically clamped to the range.
		/// </remarks>
		/// <param name="value">The value within the range</param>
		/// <returns>A value between 0 and 1, representing where the <paramref name="value"/>
		/// parameter  falls within the range.</returns>

		public float GetPercentageOf(float value)
		{
			return Mathf.InverseLerp(From, To, value);
		}

		/// <summary>
		/// Remap a value within this range to another range.
		/// </summary>
		/// <remarks>
		/// The input value is automatically clamped to this range.
		/// </remarks>
		/// <param name="value">A value within the range</param>
		/// <param name="outputRange">The range that the value should be remapped to</param>
		/// <returns>The remapped value</returns>
		public float Remap(float value, ValueRange outputRange)
		{
			return outputRange.GetValueAt(GetPercentageOf(value));
		}

		/// <summary>
		/// Clamps a value to the range.
		/// </summary>
		/// <remarks>
		/// Values are considered as inclusive of the range boundaries.
		/// </remarks> 
		/// <param name="value">The value that should be clamped to the range</param>
		/// <returns>The clamped value</returns>
		public float Clamp(float value)
		{
			return Mathf.Clamp(value, LowerBoundary, UpperBoundary);
		}

		/// <summary>
		/// Ping pong a value within the range, so that exceeding values
		/// will be wrapped back into the range.
		/// </summary>
		/// <remarks>
		/// Values are considered as inclusive of the range boundaries.
		/// </remarks>
		/// <param name="value">The value that should be ping ponged in the range</param>
		/// <returns>The ping ponged value</returns>
		public float PingPong(float value)
		{
			float lower = LowerBoundary;
			return Mathf.PingPong(value - lower, Size) + lower;
		}

		/// <summary>
		/// Loop a value within the range, so that exceeding values
		/// will be wrapped to the other side of the range.
		/// </summary>
		/// <remarks>
		/// Values are considered as inclusive of the range boundaries.
		/// </remarks>
		/// <param name="value">The value that should be looped in the range</param>
		/// <returns>The looped value</returns>
		public float Loop(float value)
		{
			return Loop(value, LowerBoundary, UpperBoundary);
		}

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

		/// <summary>
		/// Get a random value within the range, inclusive of the boundaries
		/// </summary>
		/// <returns>The random number</returns>
		public float GetRandomValue()
		{
			return GetValueAt(Random.value);
		}

		/// <summary>
		/// Checks whether a value is within the range, inclusive of the boundaries
		/// </summary>
		/// <param name="value">The value</param>
		/// <returns>Whether the value is within the range</returns>
		public bool Contains(float value)
		{
			return (value >= LowerBoundary && value <= UpperBoundary);
		}

		/// <summary>
		/// Returns a new range with both boundaries offset by the given value.
		/// </summary>
		/// <param name="offsetValue">The value to offset the range with</param>
		/// <returns>A new offset <see cref="ValueRange"/></returns>
		public ValueRange Offset(float offsetValue)
		{
			return new ValueRange(From + offsetValue, To + offsetValue);
		}

		/// <summary>
		/// Returns a new range whose size is scaled by the given value, symmetrically around the midpoint.
		/// A negative scale value flips the direction of the range.
		/// </summary>
		/// <param name="scaleValue">The value to scale the range. May be negative to flip the range.</param>
		/// <returns>A new scaled <see cref="ValueRange"/></returns>
		public ValueRange Scale(float scaleValue)
		{
			float sizeDelta = (Size - (Size * scaleValue)) * 0.5f;

			if (Direction == RangeDirection.Descending)
			{
				sizeDelta = -sizeDelta;
			}

			return new ValueRange(From + sizeDelta, To - sizeDelta);
		}

		public static ValueRange operator +(ValueRange range, float value)
		{
			return range.Offset(value);
		}

		public static ValueRange operator -(ValueRange range, float value)
		{
			return range.Offset(-value);
		}

		public static ValueRange operator *(ValueRange range, float scale)
		{
			return range.Scale(scale);
		}

		public static ValueRange operator /(ValueRange range, float scale)
		{
			return range.Scale(1 / scale);
		}

		public void Deconstruct(out float from, out float to)
		{
			(from, to) = (From, To);
		}

		public bool Equals(ValueRange other)
		{
			return From.Equals(other.From) && To.Equals(other.To);
		}

		public override bool Equals(object obj)
		{
			return obj is ValueRange other && Equals(other);
		}

		public override int GetHashCode()
		{
			return (From, To).GetHashCode();
		}

		public override string ToString() => ToString(null, null);

		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (string.IsNullOrEmpty(format))
				format = "F2";
			if (formatProvider == null)
				formatProvider = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
			// ReSharper disable once UseStringInterpolation
			return string.Format("(From: {0}, To: {1})",
				From.ToString(format, formatProvider),
				To.ToString(format, formatProvider));
		}
	}

	public enum RangeDirection
	{
		Descending = -1,
		Ascending = 1,
	}
}

#if UNITY_EDITOR
namespace dyvoid.Utility
{
	using UnityEditor;

	[CustomPropertyDrawer(typeof(ValueRange))]
	public class RangeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			using (new EditorGUI.PropertyScope(position, label, property))
			{
				// Get position
				position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

				var previousIndentLevel = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;

				// Calculate rects
				var fromRect = position;
				fromRect.width = position.width / 2f - 4;
				var toRect = fromRect;
				toRect.x = toRect.xMax + 4;

				var fromProp = property.FindPropertyRelative("From");
				var toProp = property.FindPropertyRelative("To");
				EditorGUIUtility.labelWidth = 36;
				EditorGUI.PropertyField(fromRect, fromProp, new GUIContent("From:"));
				EditorGUI.PropertyField(toRect, toProp, new GUIContent("To:"));

				EditorGUI.indentLevel = previousIndentLevel;
			}
		}
	}
}
#endif
