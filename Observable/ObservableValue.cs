using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Utility
{
	/// <summary>
	/// Represents a value that can be observed for changes.
	/// Notifies subscribers when its value is modified.
	/// </summary>
	/// <typeparam name="T">The type of the value being observed.</typeparam>
	public class ObservableValue<T> : IDisposable, ISerializationCallbackReceiver
	{
		// This field is required for inheriting classes to be serializable
		[SerializeField] protected T value;

		/// <summary>
		/// Action triggered when the value changes.
		/// The first parameter is the new value, the second is the old value.
		/// </summary>
		public Action<T, T> ChangedAction;

		/// <summary>
		/// Initializes a new instance of the <see cref="ObservableValue{T}"/> class.
		/// </summary>
		/// <param name="value">The initial value. This parameter is optional.</param>
		public ObservableValue([Optional] T value)
		{
			this.value = value;
		}

		/// <summary>
		/// Gets or sets the current value.
		/// Setting this property will trigger the <see cref="ChangedAction"/> if the new value is different from the current value.
		/// </summary>
		public virtual T Value
		{
			get { return value; }
			set
			{
				if (EqualityComparer<T>.Default.Equals(this.value, value)) return;

				T oldValue = this.value;
				this.value = value;

				this.OnValueChanged(this.value, oldValue);
			}
		}

		/// <summary>
		/// Creates a new <see cref="ObservableValue{T2}"/> that is derived from this one, transforming its value.
		/// The new observable's value is updated whenever this observable's value changes, using the provided handler function.
		/// Useful for creating dependent observables, e.g., an ObservableString derived from an ObservableInt, or an ObservableBool (isAlive) from an ObservableInt (health).
		/// </summary>
		/// <typeparam name="T2">The type of the new observable value.</typeparam>
		/// <param name="handler">A function that takes the new and old value of this observable and returns the new value for the mapped observable. The initial call to the handler will use the current value for both 'new' and 'old' parameters.</param>
		/// <returns>A new <see cref="ObservableValue{T2}"/> whose value is mapped from this observable.</returns>
		public ObservableValue<T2> Map<T2>(Func<T, T, T2> handler)
		{
			ObservableValue<T2> mappedValue = new ObservableValue<T2>(handler.Invoke(value, value));
			Subscribe((to, from) => mappedValue.Value = handler.Invoke(to, from), true);
			return mappedValue;
		}

		/// <summary>
		/// Merges two <see cref="ObservableValue{T}"/> instances into a new one.
		/// The new observable's value is updated whenever either of the source observables changes, using the provided combinator function.
		/// Useful for creating an observable whose state depends on multiple other observables, e.g., an ObservableBool (canInteract) that is true if ObservableBool (isInRange) and ObservableBool (hasRequiredItem) are both true.
		/// </summary>
		/// <typeparam name="T1">The type of the first observable value.</typeparam>
		/// <typeparam name="T2">The type of the second observable value.</typeparam>
		/// <typeparam name="T3">The type of the resulting merged observable value.</typeparam>
		/// <param name="value1">The first observable value.</param>
		/// <param name="value2">The second observable value.</param>
		/// <param name="combinator">A function that combines the current values of the two source observables to produce the value for the merged observable.</param>
		/// <returns>A new <see cref="ObservableValue{T3}"/> whose value is a combination of the two source observables.</returns>
		public static ObservableValue<T3> Merge<T1, T2, T3>(ObservableValue<T1> value1, ObservableValue<T2> value2, Func<T1, T2, T3> combinator)
		{
			ObservableValue<T3> c = new ObservableValue<T3>(combinator.Invoke(value1.Value, value2.Value));
			value1.Subscribe((to, from) => c.Value = combinator.Invoke(to, value2.Value), true);
			value2.Subscribe((to, from) => c.Value = combinator.Invoke(value1.Value, to), true);
			return c;
		}

		/// <summary>
		/// Merges this <see cref="ObservableValue{T}"/> with another one.
		/// The new observable's value is updated whenever either this or the other observable changes, using the provided combinator function.
		/// This is an instance method version of <see cref="Merge{T1, T2, T3}"/>. Useful for creating an observable whose state depends on multiple other observables, e.g., an ObservableBool (canInteract) that is true if this ObservableBool (isInRange) and another ObservableBool (hasRequiredItem) are both true.
		/// </summary>
		/// <typeparam name="T2">The type of the other observable value.</typeparam>
		/// <typeparam name="T3">The type of the resulting merged observable value.</typeparam>
		/// <param name="value">The other observable value to merge with.</param>
		/// <param name="combinator">A function that combines the current values of this and the other observable to produce the value for the merged observable.</param>
		/// <returns>A new <see cref="ObservableValue{T3}"/> whose value is a combination of this and the other observable.</returns>
		public ObservableValue<T3> MergeWith<T2, T3>(ObservableValue<T2> value, Func<T, T2, T3> combinator)
		{
			return Merge(this, value, combinator);
		}

		/// <summary>
		/// Subscribes a callback to be invoked when the <see cref="Value"/> changes.
		/// </summary>
		/// <param name="callback">The action to invoke. The first parameter is the new value, the second is the old value.</param>
		/// <param name="getInitialValue">If true, the callback is immediately invoked with the current value (passed as both new and old value).</param>
		/// <returns>This <see cref="ObservableValue{T}"/> instance, allowing for fluent chaining.</returns>
		public ObservableValue<T> Subscribe(Action<T, T> callback, bool getInitialValue = false)
		{
			ChangedAction += callback;

			if (getInitialValue)
			{
				callback?.Invoke(value, value);
			}

			return this;
		}

		/// <summary>
		/// Unsubscribes a callback from value change notifications.
		/// </summary>
		/// <param name="callback">The action to unsubscribe.</param>
		/// <returns>This <see cref="ObservableValue{T}"/> instance, allowing for fluent chaining.</returns>
		public ObservableValue<T> Unsubscribe(Action<T, T> callback)
		{
			if (ChangedAction != null)
			{
				ChangedAction -= callback;
			}

			return this;
		}

		/// <summary>
		/// Invoked when the value has changed. Triggers the <see cref="ChangedAction"/>.
		/// </summary>
		/// <param name="newValue">The new value.</param>
		/// <param name="oldValue">The old value.</param>
		protected void OnValueChanged(T newValue, T oldValue)
		{
			ChangedAction?.Invoke(newValue, oldValue);
		}

		/// <summary>
		/// Releases all resources used by the <see cref="ObservableValue{T}"/> object.
		/// Specifically, it clears all subscribers from <see cref="ChangedAction"/>.
		/// </summary>
		public void Dispose()
		{
			ChangedAction = null;
		}

		#region ISerializationCallbackReceiver

		/// <summary>
		/// Unity's serialization callback, invoked before serialization.
		/// </summary>
		public virtual void OnBeforeSerialize()
		{

		}

		/// <summary>
		/// Unity's serialization callback, invoked after deserialization.
		/// </summary>
		public virtual void OnAfterDeserialize()
		{

		}

		#endregion
	}
}
