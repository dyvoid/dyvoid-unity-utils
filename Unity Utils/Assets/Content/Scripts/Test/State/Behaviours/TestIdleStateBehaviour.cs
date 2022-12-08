using MediaMonks.State;
using UnityEngine;

namespace Test
{
    public class TestIdleStateBehaviour : TestStateBehaviour
	{
        public override TestState State => TestState.Idle;

        public override void OnEnter(TestState prevState)
        {
	        Debug.Log("Idle OnEnter");
        }
    }
}