using System;
using Object = UnityEngine.Object;

namespace MediaMonks.References
{
	/// <summary>
	/// Typed base for ScriptableObjects that store a reference to an object instance.
	/// You can't set it to a new value without clearing it first.
	/// </summary>
	public class ReferenceObject<T> : ScriptableObjectReferenceBase where T : class
	{
		private T _value;

		private Action<T> _whenValueSet;

		/// <inheritdoc/>
		public override string EditorDescription =>
			$"<b>Description</b>\nRuntime reference to a <b>{typeof(T).Name}</b> instance.";

		/// <summary>
		/// Returns the <see cref="Type"/> of the <see cref="Value"/>. Doesn't access the value, so safe to use when it has not been set.
		/// </summary>
		public override Type GetTypeOfValue()
		{
			return typeof(T);
		}

		/// <inheritdoc/>
		public override bool TrySetValue(object value)
		{
			if (!(value is T typedValue)) return false;
			SetValue(typedValue);
			return true;
		}

		/// <inheritdoc/>
		public override bool TryGetValue<TOut>(out TOut value)
		{
			if (HasValue)
			{
				if (Value is TOut outValue)
				{
					value = outValue;
					return true;
				}
			}

			value = default;
			return false;
		}

		/// <summary>
		/// Returns the value.
		/// </summary>
		/// <exception cref="Exception">Throws exception when the value hasn't been set.</exception>
		public T Value
		{
			get
			{
				if (_value == null)
				{
					throw new Exception($"Value has not been set: {name}");
				}

				return _value;
			}
		}

		/// <summary>
		/// Returns if the <see cref="Value"/> has been set.
		/// </summary>
		public bool HasValue
		{
			get
			{
				return _value switch
				{
					Object objectValue => objectValue != null,
					_ => _value != null
				};
			}
		}

		/// <summary>
		/// Sets the <see cref="Value"/>.
		/// </summary>
		/// <exception cref="Exception">Throws exception when the value is already set.</exception>
		public void SetValue(T value)
		{
			if (HasValue)
			{
				throw new Exception($"Value has already been set: {name}");
			}

			_value = value;
			_whenValueSet?.Invoke(_value);
			_whenValueSet = null;
		}

		/// <summary>
		/// Perform the callback when the <see cref="Value"/> is set. If already set, it is performed immediately.
		/// </summary>
		public void WhenValueSet(Action<T> whenSetCallback)
		{
			if (HasValue)
			{
				whenSetCallback.Invoke(Value);
			}
			else
			{
				_whenValueSet += whenSetCallback;
			}
		}

		/// <summary>
		/// Clears assigned <see cref="Value"/>.
		/// </summary>
		public void ClearValue()
		{
			_value = null;
		}

		protected override void OnFinalize()
		{
			ClearValue();
		}
	}

	public static class ReferenceObjectExtensions
	{
		/// <summary>
		/// <inheritdoc cref="ReferenceObject.HasValue"/> Is safe to use when the ReferenceObject is null.
		/// </summary>
		public static bool NullSafeHasValue<T>(this ReferenceObject<T> reference) where T : class
		{
			return reference != null && reference.HasValue;
		}

		/// <summary>
		/// <inheritdoc cref="ReferenceObject.ClearValue"/> Is safe to use when the ReferenceObject is null.
		/// </summary>
		public static void NullSafeClearValue<T>(this ReferenceObject<T> reference) where T : class
		{
			if (reference.NullSafeHasValue())
			{
				reference.ClearValue();
			}
		}
		
		/// <summary>
		/// <inheritdoc cref="ReferenceObject.Value"/> Is safe to use when the ReferenceObject is null.
		/// </summary>
		public static T NullSafeGetValue<T>(this ReferenceObject<T> reference) where T : class
		{
			return reference.NullSafeHasValue() ? reference.Value : default;
		}

		/// <summary>
		/// <inheritdoc cref="ReferenceObject.TryGetValue"/> Is safe to use when the ReferenceObject is null.
		/// </summary>
		public static bool NullSafeTryGetValue<T>(this ReferenceObject<T> reference, out T value) where T : class
		{
			if (reference != null) return reference.TryGetValue(out value);

			value = default;
			return false;
		}
	}
}