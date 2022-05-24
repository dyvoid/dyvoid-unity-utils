using System;
using UnityEngine;

namespace Temple.State
{
    public abstract class AbstractStateBehaviour<T> : MonoBehaviour, IStateBehaviour<T>
    {
        public bool IsActive { get; private set; }

        public abstract T State { get; }

        protected IStateMachine<T> StateMachine;


        protected virtual void Awake()
        {
            StateMachine = GetComponent<IStateMachine<T>>();
        }

        public void ChangeState(T state)
        {
            StateMachine.ChangeState(state);
        }

        public virtual void OnEnter()
        {
            IsActive = true;
        }

        public virtual void OnExit()
        {
            IsActive = false;
        }

        public virtual void OnUpdateState()
        {

        }

        public virtual void OnFixedUpdateState()
        {

        }
    }
}