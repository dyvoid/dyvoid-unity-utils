using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace dyvoid.Utility
{
	/// <summary>
	/// Represents a value that can be observed for changes.
	/// Notifies subscribers when its value is modified.
	/// </summary>
	/// <remarks>
	/// Implements <see cref="IDisposable"/> but calling <see cref="Dispose"/> is optional.
	/// For plain observables, subscribers are cleaned up by GC when the owner is collected.
	/// Call <see cref="Dispose"/> when you want to explicitly sever all subscriptions, or
	/// when disposing of a <c>Map</c>/<c>Merge</c> result to detach it from its source(s).
	/// </remarks>
	/// <typeparam name="T">The type of the value being observed.</typeparam>
	[Serializable]
	public class ObservableValue<T> : IDisposable, ISerializationCallbackReceiver
	{
		// This field is required for inheriting classes to be serializable
		[SerializeField] protected T value;

		private Action<T, T> changedAction;

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
		/// Setting this property will notify subscribers if the new value is different from the current value.
		/// </summary>
		/// <remarks>
		/// Avoid setting <see cref="Value"/> from within a subscriber callback: doing so causes
		/// depth-first recursive notifications with interleaved old/new pairs.
		/// </remarks>
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
			return new MappedObservableValue<T, T2>(this, handler);
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
			return new MergedObservableValue<T1, T2, T3>(value1, value2, combinator);
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
			bool alreadySubscribed = (changedAction != null && Array.IndexOf(changedAction.GetInvocationList(), (Delegate)callback) >= 0);

			if (!alreadySubscribed)
			{
				changedAction += callback;
			}

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
			if (changedAction != null)
			{
				changedAction -= callback;
			}

			return this;
		}

		/// <summary>
		/// Invoked when the value has changed. Triggers the change notification.
		/// </summary>
		/// <param name="newValue">The new value.</param>
		/// <param name="oldValue">The old value.</param>
		protected void OnValueChanged(T newValue, T oldValue)
		{
			changedAction?.Invoke(newValue, oldValue);
		}

		/// <summary>
		/// Releases all resources used by the <see cref="ObservableValue{T}"/> object.
		/// Specifically, it clears all subscribers.
		/// </summary>
		public virtual void Dispose()
		{
			changedAction = null;
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
