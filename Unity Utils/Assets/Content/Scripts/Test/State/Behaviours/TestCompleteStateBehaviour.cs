using MediaMonks.State;
using UnityEngine;

namespace Test
{
    public class TestCompleteStateBehaviour : TestStateBehaviour
	{
        public override TestState State => TestState.Complete;

        public override void OnEnter(TestState prevState)
        {
	        Debug.Log("Complete OnEnter");
        }
    }
}