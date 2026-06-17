using UnityEngine;

namespace dyvoid.Utility.Editor
{
	public static class FSMWizardTemplates
	{
		public const string EnumTemplate = @"#NAMESPACE_START#
	public enum #FSM_NAME#State
	{
#STATES#
	}
#NAMESPACE_END#";

		public const string FSMClassTemplate = @"using dyvoid.Utility;

#NAMESPACE_START#
	public class #FSM_NAME#FSM : FSM<#FSM_NAME#State>
	{
	}
#NAMESPACE_END#";

		public const string BlackboardTemplate = @"using dyvoid.Utility;

#NAMESPACE_START#
	public class #FSM_NAME#Blackboard : FSMBlackboard
	{
		// Add shared references and data here.
	}
#NAMESPACE_END#";

		public const string VisibilityTemplate = @"using dyvoid.Utility;
using UnityEngine;

#NAMESPACE_START#
	public class #FSM_NAME#StateVisibility : FSMStateVisibility<#FSM_NAME#State>
	{
		[SerializeField] private #FSM_NAME#FSM fsm;

		protected override ObservableValue<#FSM_NAME#State> GetStateObservable()
		{
			return fsm.StateObservable;
		}
	}
#NAMESPACE_END#";

		public const string StateBehaviourTemplate = @"using dyvoid.Utility;

#NAMESPACE_START#
	public class #FSM_NAME##STATE#StateBehaviour : #BASE_CLASS#
	{
		public override #FSM_NAME#State State => #FSM_NAME#State.#STATE#;

		public override void OnEnter(#FSM_NAME#State prevState)
		{
		}

		public override void OnExit(#FSM_NAME#State nextState)
		{
		}

		public override void OnUpdateState()
		{
		}
	}
#NAMESPACE_END#";

		public const string AllStateBehavioursTemplate = @"using dyvoid.Utility;

#NAMESPACE_START#
#STATE_CLASSES#
#NAMESPACE_END#";

		public const string SingleStateClassTemplate = @"	public class #FSM_NAME##STATE#StateBehaviour : #BASE_CLASS#
	{
		public override #FSM_NAME#State State => #FSM_NAME#State.#STATE#;

		public override void OnEnter(#FSM_NAME#State prevState)
		{
		}

		public override void OnExit(#FSM_NAME#State nextState)
		{
		}

		public override void OnUpdateState()
		{
		}
	}";
	}
}
