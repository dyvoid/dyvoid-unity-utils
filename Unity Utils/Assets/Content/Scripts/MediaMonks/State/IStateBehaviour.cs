using System;

namespace MediaMonks.State
{
    public interface IStateBehaviour<T> where T : Enum
    {
        T State { get; }
        string StateName { get; }

        bool IsActive { get; }

        void OnEnter(T prevState);

        void OnExit(T nextState);

        void OnUpdateState();

        void OnFixedUpdateState();
    }
}
