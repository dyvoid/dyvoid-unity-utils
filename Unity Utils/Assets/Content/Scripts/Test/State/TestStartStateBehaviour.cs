using Temple.State;
using UnityEngine;

namespace Test
{
    public class TestStartStateBehaviour : AbstractStateBehaviour<TestState>
    {
        public override TestState State => TestState.Start;

        public override void OnEnter()
        {
            base.OnEnter();

            Debug.Log("Start OnEnter");
        }

        public override void OnUpdateState()
        {
            base.OnUpdateState();

            Debug.Log("Start state running");
        }
    }
}