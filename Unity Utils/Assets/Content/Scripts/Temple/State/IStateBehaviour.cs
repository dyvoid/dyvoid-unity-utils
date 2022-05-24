namespace Temple.State
{
    public interface IStateBehaviour<T>
    {
        T State { get; }

        bool IsActive { get; }

        void OnEnter();

        void OnExit();

        void ChangeState(T state);

        void OnUpdateState();

        void OnFixedUpdateState();
    }
}