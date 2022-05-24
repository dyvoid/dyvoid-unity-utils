using System;
using UnityEngine;

namespace Temple.State
{
    public abstract class AbstractStateBehaviour<T> : AbstractStateBehaviourBase, IStateBehaviour<T> where T : Enum
    {
        public abstract T State { get; }
        public override string StateName => State.ToString();

        protected IStateMachine<T> StateMachine;


        protected virtual void Awake()
        {
            StateMachine = GetComponent<IStateMachine<T>>();
        }

        public void ChangeState(T state)
        {
            StateMachine.ChangeState(state);
        }

        public override void ChangeToThisState()
        {
            ChangeState(State);
        }

    }
}