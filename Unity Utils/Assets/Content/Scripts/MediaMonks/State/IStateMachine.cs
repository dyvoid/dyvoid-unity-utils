using System;
using MediaMonks.Utility;

namespace MediaMonks.State
{
    public interface IStateMachine<T> where T : Enum
    {
	    ObservableValue<T> StateObservable { get; }
        T CurrentState { get; }

        IStateBehaviour<T> CurrentBehaviour { get; }
        IStateBehaviour<T> PreviousBehaviour { get; }

        IStateBehaviour<T> GetBehavior(T state);

        void ChangeState(T newState);
    }
}
