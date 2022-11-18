using System;
using System.Collections.Generic;
using Temple.Utils;
using UnityEngine;

namespace Temple.State
{
	/// <summary>
	/// A finite state machine, that automatically triggers behaviours on state changes.
	/// </summary>
	/// <typeparam name="T">The enum type of the state</typeparam>
	public abstract class AbstractStateMachine<T> : MonoBehaviour, IStateMachine<T> where T : Enum
    {
        [SerializeField] private bool _logStateChanges = false;
        [SerializeField] private T _startState;

        public ObservableValue<T> StateObservable { get; } = new();

        public IStateBehaviour<T> CurrentBehaviour { get; private set; }
        public IStateBehaviour<T> PreviousBehaviour { get; private set; }

        public T CurrentState => StateObservable.Value;
        public T PreviousState => (PreviousBehaviour != null) ? PreviousBehaviour.State : default(T);

		private readonly Dictionary<T, IStateBehaviour<T>> _behaviourMap = new Dictionary<T, IStateBehaviour<T>>();


        protected virtual void Start()
        {
			// Fetch all the state behaviours on this game object
            foreach (IStateBehaviour<T> behavior in gameObject.GetComponents<IStateBehaviour<T>>())
            {
                _behaviourMap.Add(behavior.State, behavior);
            }

			// Init the observable
            StateObservable.Value = _startState;
			StateObservable.ChangedWithCurrentValue += HandleStateChanged;
		}

		/// <summary>
		/// Get a behavior that is registered with this state machine.
		/// </summary>
		/// <param name="state">The state of the behaviour to get</param>
		/// <returns>The state behaviour</returns>
        public IStateBehaviour<T> GetBehavior(T state)
        {
            return _behaviourMap[state];
        }

		/// <summary>
		/// Change to a new state.
		/// </summary>
		/// <param name="newState">The state to change to</param>
        public void ChangeState(T newState)
        {
	        StateObservable.Value = newState;
        }

        protected virtual void HandleStateChanged(T newState)
        {
            if (_logStateChanges) Debug.Log($"[State Machine] State changed: {newState}");

			// First exit the current state behaviour, if one is active
			CurrentBehaviour?.OnExit();

            PreviousBehaviour = CurrentBehaviour;

	        // Now attempt to enter the new state behaviour, if it is available
			if (_behaviourMap.TryGetValue(newState, out var newStateBehaviour))
			{
				CurrentBehaviour = newStateBehaviour;
                CurrentBehaviour.OnEnter();
            }
            else
            {
                CurrentBehaviour = null;
            }
        }

        protected virtual void Update()
        {
			if (CurrentBehaviour != null) CurrentBehaviour.OnUpdateState();
        }

        protected virtual void FixedUpdate()
        {
	        if (CurrentBehaviour != null) CurrentBehaviour.OnFixedUpdateState();
        }

        protected virtual void OnDestroy()
        {
	        StateObservable.ChangedWithCurrentValue -= HandleStateChanged;
        }
	}
}
