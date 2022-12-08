using MediaMonks.State;

namespace Test
{
	public abstract class TestStateBehaviour : AbstractStateBehaviour<TestState>
	{
		public override TestState State { get; }

		protected TestBlackboard Blackboard => base.Blackboard as TestBlackboard;
	}
}