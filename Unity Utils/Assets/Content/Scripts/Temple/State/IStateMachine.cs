using System;
using MediaMonks.References;

namespace Temple.State
{
    public interface IStateMachine<T> where T : Enum
    {
        ReferenceValue<T> StateReference { get; }
        T CurrentState { get; }

        IStateBehaviour<T> CurrentBehaviour { get; }
        IStateBehaviour<T> PreviousBehaviour { get; }

        IStateBehaviour<T> GetBehavior(T state);

        void ChangeState(T newState);
    }
}