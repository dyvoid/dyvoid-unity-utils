using MediaMonks.State;
using UnityEngine;

namespace Test
{
    public class TestCompleteStateBehaviour : AbstractStateBehaviour<TestState>
    {
        public override TestState State => TestState.Complete;

        public override void OnEnter()
        {
            base.OnEnter();

            Debug.Log("Complete OnEnter");
        }
    }
}