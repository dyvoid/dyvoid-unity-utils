using System;
using System.Collections.Generic;
using UnityEngine;

namespace Temple.Utils
{
	/// <summary>
	/// Simple serializable observable value that emits its current value on subscription and raises an event when its
	/// value changes
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public class ObservableValue<T> : IReadOnlyObservableValue<T>
	{
		public delegate void PropertyChangedDelegate(T value);

		/// <summary>
		/// Gets and set the value which is then propagated through its events. Not that the event is
		/// only raised when the value differs from the current value.
		/// </summary>
		public virtual T Value
		{
			get => _value;
			set
			{
				bool changed = !_comparer.Equals(_value, value);
				_value = value;
				if (changed) _propertyChanged?.Invoke(_value);
			}
		}

		public event PropertyChangedDelegate ChangedWithCurrentValue
		{
			add
			{
				value.Invoke(_value);
				_propertyChanged += value;
			}
			remove
			{
				_propertyChanged -= value;
			}
		}

		public event PropertyChangedDelegate Changed
		{
			add
			{
				_propertyChanged += value;
			}
			remove
			{
				_propertyChanged -= value;
			}
		}

		private PropertyChangedDelegate _propertyChanged;
		private static readonly EqualityComparer<T> _comparer = EqualityComparer<T>.Default;

		[SerializeField] private T _value;

		public ObservableValue() : this(default) { }

		public ObservableValue(T initialValue)
		{
			_value = initialValue;
		}

		public void SetValueAndForceNotify(T value)
		{
			_value = value;
			_propertyChanged?.Invoke(value);
		}
	}

	public interface IReadOnlyObservableValue<T>
	{
		T Value { get; }
		event ObservableValue<T>.PropertyChangedDelegate ChangedWithCurrentValue;
		event ObservableValue<T>.PropertyChangedDelegate Changed;
	}

	public static class ObservableValueExtensions
	{
		public static void Clear<T>(this ObservableValue<T> observableValue, T value) where T : class
		{
			if (value == null || !value.Equals(observableValue.Value)) return;

			observableValue.Value = null;
		}
	}
}
