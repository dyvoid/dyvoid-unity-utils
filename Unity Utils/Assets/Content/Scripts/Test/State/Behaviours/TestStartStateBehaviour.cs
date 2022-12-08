using MediaMonks.State;
using UnityEngine;

namespace Test
{
    public class TestStartStateBehaviour : TestStateBehaviour
    {
        public override TestState State => TestState.Start;


        public override void OnEnter(TestState prevState)
        {
			Debug.Log("Start OnEnter");
        }

        public override void OnUpdateState()
        {
            base.OnUpdateState();

            // Debug.Log("Start state running");

            Debug.Log(Blackboard.Dus.Value);
        }
    }
}