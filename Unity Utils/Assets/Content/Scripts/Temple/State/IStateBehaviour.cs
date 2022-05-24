using System;

namespace Temple.State
{
    public interface IStateBehaviour<T> where T : Enum
    {
        T State { get; }
        string StateName { get; }

        bool IsActive { get; }

        void OnEnter();

        void OnExit();

        void ChangeState(T state);

        void OnUpdateState();

        void OnFixedUpdateState();
    }
}