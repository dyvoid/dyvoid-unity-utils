using System;
using System.Collections.Generic;
using UnityEngine;

namespace Temple.State
{
    public abstract class AbstractStateMachine<T> : StateChangeListener<T>, IStateMachine<T> where T : Enum
    {
        [SerializeField] private bool _logStateChanges = false;
        [SerializeField] private T _startState;

        public T CurrentState => StateReference.Value;
        public T PreviousState {get; private set;}

        public IStateBehaviour<T> CurrentBehaviour { get; private set; }
        public IStateBehaviour<T> PreviousBehaviour { get; private set; }

        private readonly Dictionary<T, IStateBehaviour<T>> _behaviourMap = new Dictionary<T, IStateBehaviour<T>>();


        protected override void Start()
        {
            foreach (IStateBehaviour<T> behavior in gameObject.GetComponents<IStateBehaviour<T>>())
            {
                _behaviourMap.Add(behavior.State, behavior);
            }

            base.Start();

            if (!StateReference.Value.Equals(_startState))
            {
                ChangeState(_startState);
            }
            else
            {
                OnStateChanged(_startState, StateReference.Value);
            }
        }

        public IStateBehaviour<T> GetBehavior(T state)
        {
            return _behaviourMap[state];
        }

        public void ChangeState(T newState)
        {
            StateReference.Value = newState;
        }

        protected override void OnStateChanged(T newState, T oldState)
        {
            if (_logStateChanges) Debug.Log($"[State Machine] State changed: {oldState} -> {newState}");

            PreviousBehaviour = CurrentBehaviour;
            PreviousState = oldState;

            CurrentBehaviour?.OnExit();

            if (_behaviourMap.ContainsKey(newState))
            {
                CurrentBehaviour = _behaviourMap[newState];
                CurrentBehaviour.OnEnter();
            }
            else
            {
                CurrentBehaviour = null;
            }
        }

        protected virtual void Update()
        {
            CurrentBehaviour?.OnUpdateState();
        }

        protected virtual void FixedUpdate()
        {
            CurrentBehaviour?.OnFixedUpdateState();
        }
    }
}
