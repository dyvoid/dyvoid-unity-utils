using Temple.State;
using UnityEngine;

namespace Test
{
    public class TestIdleStateBehaviour : AbstractStateBehaviour<TestState>
    {
        public override TestState State => TestState.Idle;

        public override void OnEnter()
        {
            base.OnEnter();

            Debug.Log("Idle enter");
        }
    }
}