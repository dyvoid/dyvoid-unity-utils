using System;
using System.Collections.Generic;
using UnityEngine;
using dyvoid.Utility;

namespace dyvoid.Utility
{
	/// <summary>
	/// Controls GameObject visibility based on enum state changes from any observable source.
	/// While part of the FSM framework, it works with any ObservableValue that emits enum values.
	/// </summary>
	/// <typeparam name="TState">The enum type to observe for visibility changes</typeparam>
	/// <remarks>
	/// This class provides a foundation for creating state-based visibility controllers.
	/// It automatically subscribes to enum value changes and manages visibility based on the current value.
	/// 
	/// While designed for use with the FSM framework, this component makes no assumptions about
	/// your state management implementation. You must provide the state observable through GetStateObservable(),
	/// which gives you complete flexibility - it could be from an FSM, a simple state machine, or any
	/// other system that provides an observable enum value.
	/// 
	/// Example implementation:
	/// <code>
	/// public class PlayerStateVisibility : FSMStateVisibility<PlayerState> {
	///     [SerializeField] private PlayerFSM playerFSM;  // Assign in Inspector
	///     
	///     protected override ObservableValue<PlayerState> GetStateObservable() {
	///         return playerFSM.StateObservable;
	///     }
	/// }
	/// </code>
	/// 
	/// Then add the component to a GameObject and configure the visible states in the Inspector.
	/// </remarks>
	public abstract class FSMStateVisibility<TState> : MonoBehaviour where TState : Enum
	{
		[SerializeField] private bool invertVisibility = false;
		[SerializeField] private List<TState> states = new List<TState>();

		protected ObservableValue<TState> stateObservable;

		protected virtual void Awake()
		{
			Initialize(GetStateObservable());
		}

		/// <summary>
		/// Provides the observable state value that this component will react to.
		/// </summary>
		/// <returns>An ObservableValue of type T that emits state changes</returns>
		/// <remarks>
		/// This is the key method that connects the visibility controller to your FSM.
		/// Implementations must return an ObservableValue that emits values of the same enum type T
		/// that this class is parameterized with.
		/// 
		/// The returned observable will be automatically subscribed and properly
		/// unsubscribed from when needed.
		/// </remarks>
		protected abstract ObservableValue<TState> GetStateObservable();

		/// <summary>
		/// Initialize the controller with an observable state value.
		/// </summary>
		private void Initialize(ObservableValue<TState> observable)
		{
			// Unsubscribe from previous observable if any
			Unsubscribe();

			stateObservable = observable;

			// Subscribe to new observable
			Subscribe();
		}

		protected void Subscribe()
		{
			stateObservable?.Subscribe(HandleStateChanged, true);
		}

		/// <summary>
		/// Unsubscribe from state changes.
		/// </summary>
		protected void Unsubscribe()
		{
			stateObservable?.Unsubscribe(HandleStateChanged);
		}

		/// <summary>
		/// Handle state changes by updating visibility.
		/// </summary>
		protected virtual void HandleStateChanged(TState newState, TState oldState)
		{
			UpdateVisibility(newState);
		}

		/// <summary>
		/// Update the GameObject visibility based on the current state.
		/// </summary>
		protected virtual void UpdateVisibility(TState state)
		{
			bool shouldBeVisible = ShouldBeVisible(state);
			gameObject.SetActive(shouldBeVisible);
		}

		/// <summary>
		/// Determine if the GameObject should be visible in the given state.
		/// </summary>
		protected virtual bool ShouldBeVisible(TState state)
		{
			bool isStateInList = states.Contains(state);
			return invertVisibility ? !isStateInList : isStateInList;
		}

		protected virtual void OnDestroy()
		{
			Unsubscribe();
		}      
	}
}
