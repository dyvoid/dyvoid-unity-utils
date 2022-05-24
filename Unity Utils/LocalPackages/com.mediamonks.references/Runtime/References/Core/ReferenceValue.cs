using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace MediaMonks.References
{
	/// <summary>
	/// Typed base for <see cref="ScriptableObject"/> that stores a <see cref="Value"/>.
	/// </summary>
	public class ReferenceValue<T> : ScriptableObjectReferenceBase, IValueAsObjectHasChangedSender
	{
		[SerializeField] private T _initialValue;

		private T _value;

		/// <inheritdoc/>
		public override string EditorDescription =>
			typeof(T).IsValueType
				? $"<b>Description</b>\nRuntime reference to a <b>{typeof(T).Name}</b> value."
				: $"<b>Description</b>\nRuntime reference to a <b>{typeof(T).Name}</b> instance.";

		/// <inheritdoc/>
		protected override void Initialize()
		{
			_value = _initialValue;
		}

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
			Value = typedValue;
			return true;
		}

		/// <inheritdoc/>
		public override bool TryGetValue<TOut>(out TOut value)
		{
			if (Value is TOut outValue)
			{
				value = outValue;
				return true;
			}

			value = default;
			return false;
		}

		public delegate void ValueHasChangedDelegate(T newValue, T oldValue);

		/// <summary>
		/// Invoked when the <see cref="Value"/> changes.
		/// </summary>
		public event ValueHasChangedDelegate ValueHasChanged;

		/// <summary>
		/// Invoked when the <see cref="Value"/> changes.
		/// </summary>
		public event IValueAsObjectHasChangedSender.ValueAsObjectHasChangedDelegate ValueAsObjectHasChanged;

		/// <summary>
		/// Initial <see cref="Value"/>.
		/// </summary>
		public T InitialValue
		{
			get => _initialValue;
			set => _initialValue = value;
		}

		public delegate T FilterDelegate(T value);

		/// <summary>
		/// No filter, output value is the same as the input value.
		/// </summary>
		public static readonly FilterDelegate NoFilter = value => value;

		private FilterDelegate _inputFilter = NoFilter;
		private FilterDelegate _outputFilter = NoFilter;
		private bool _isInvokingValueHasChanged;

		private Queue<T> _queuedValueChanges;
		private bool HasQueuedValueChanges => _queuedValueChanges != null && _queuedValueChanges.Count > 0;

		/// <summary>
		/// Setting the value passes it through the <see cref="InputFilter"/>.
		/// Getting the value passes it through the <see cref="OutputFilter"/>.
		/// Invokes <see cref="ValueHasChanged"/> if the value is changed.
		/// </summary>
		public T Value
		{
			get => _outputFilter(_value);

			set
			{
				if (ValueHasChanged == null && ValueAsObjectHasChanged == null)
				{
					// No listeners, skip equality check
					_value = _inputFilter(value);
					return;
				}

				if (_isInvokingValueHasChanged)
				{
					// A new value was set while signalling a value changes, queue the change for after signalling has finished
					QueueValueChange(value);
					return;
				}

				var oldValue = Value;
				_value = _inputFilter(value);

				var outputFilteredValue = Value;

				_isInvokingValueHasChanged = true;
				if (!EqualityComparer<T>.Default.Equals(outputFilteredValue, oldValue))
				{
					ValueHasChanged?.Invoke(outputFilteredValue, oldValue);
					ValueAsObjectHasChanged?.Invoke(outputFilteredValue, oldValue);
				}

				_isInvokingValueHasChanged = false;

				if (HasQueuedValueChanges)
				{
					// Handle queued, next in line, change
					Value = _queuedValueChanges.Dequeue();
				}
			}
		}

		/// <summary>
		/// Access to the <see cref="Value"/> bypassing any of the filters.
		/// </summary>
		public T UnfilteredValue
		{
			get => _value;

			set
			{
				if (EqualityComparer<T>.Default.Equals(value, _value)) return;

				// TODO: Handle queueing of values set during invoking ValueHasChanged
				var oldValue = _value;
				_value = value;

				_isInvokingValueHasChanged = true;
				ValueHasChanged?.Invoke(_value, oldValue);
				ValueAsObjectHasChanged?.Invoke(_value, oldValue);
				_isInvokingValueHasChanged = false;
			}
		}

		/// <summary>
		/// Resets <see cref="Value"/> to <see cref="InitialValue"/>.
		/// </summary>
		public virtual void ResetValue()
		{
			Value = _initialValue;
		}

		/// <summary>
		/// Sets the input filter that is used when setting the <see cref="Value"/>.
		/// Set it to <see cref="NoFilter"/> if it shouldn't be filtered (=default)
		/// </summary>
		[NotNull]
		public FilterDelegate InputFilter
		{
			set
			{
				Assert.IsNotNull(value);
				_inputFilter = value;
			}
		}

		/// <summary>
		/// Sets the output filter that is used when getting the <see cref="Value"/>.
		/// Set it to <see cref="NoFilter"/> if it shouldn't be filtered (=default)
		/// </summary>
		[NotNull]
		public FilterDelegate OutputFilter
		{
			set
			{
				Assert.IsNotNull(value);
				_outputFilter = value;
			}
		}

		protected override void OnFinalize()
		{
			_inputFilter = NoFilter;
			_outputFilter = NoFilter;
			ValueHasChanged = null;
			ValueAsObjectHasChanged = null;
			_value = _initialValue;
			_queuedValueChanges = null;
			_isInvokingValueHasChanged = false;
		}

		private void QueueValueChange(T value)
		{
			_queuedValueChanges ??= new Queue<T>();
			_queuedValueChanges.Enqueue(value);
		}
	}
}