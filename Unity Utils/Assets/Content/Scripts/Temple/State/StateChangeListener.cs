using MediaMonks.References;
using UnityEngine;

namespace Temple.State
{
	/// <summary>
	/// Convenience base class for MonoBehaviours that want to listen for state changes.
	/// Automatically subscribes to reference value changes.
	/// </summary>
	public abstract class StateChangeListener<T> : MonoBehaviour
	{
		[SerializeField] private ReferenceValue<T> stateReference;

		public ReferenceValue<T> StateReference => stateReference;

		protected virtual void Start()
		{
			stateReference.ValueHasChanged += OnStateChanged;
			OnStateChangeListenerStart();
		}

        protected virtual void OnDestroy()
		{
			stateReference.ValueHasChanged -= OnStateChanged;
			OnStateChangeListenerDestroy();
		}

		/// <summary>
		/// Called during Start.
		/// </summary>
		protected virtual void OnStateChangeListenerStart() { }

		/// <summary>
		/// Called during OnDestroy.
		/// </summary>
		protected virtual void OnStateChangeListenerDestroy() { }

		/// <summary>
		/// Called when the state has changed.
		/// </summary>
		protected abstract void OnStateChanged(T newState, T oldState);
	}
}