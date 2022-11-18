using System;
using UnityEditor;
using UnityEngine;

namespace Temple.State
{
	/// <summary>
	/// A behaviour that runs for as long as the state machine is in a particular state.
	/// </summary>
	/// <typeparam name="T">The enum type of the state</typeparam>
	public abstract class AbstractStateBehaviour<T> : AbstractStateBehaviourBase, IStateBehaviour<T> where T : Enum
	{
		public abstract T State { get; }
		public override string StateName => State.ToString();

		private IStateMachine<T> _stateMachine;
		protected IStateMachine<T> StateMachine => _stateMachine ??= GetComponent<IStateMachine<T>>();

		/// <summary>
		/// Whether this behaviour is currently active
		/// </summary>
		public override bool IsActive
		{
			get
			{
				if (StateMachine != null)
				{
					return State.Equals(StateMachine.CurrentState);
				}
				else
				{
					Debug.LogWarning("Can't check state, because the StateMachine is not available");
					return false;
				}
			}
		}

		/// <summary>
		/// Called once by the state machine, when the state becomes active.
		/// </summary>
		public virtual void OnEnter()
		{
			
		}

		/// <summary>
		/// Called once by the state machine, when the state becomes inactive.
		/// </summary>
		public virtual void OnExit()
		{
			
		}

		/// <summary>
		/// Called continuously from the update loop of the state machine,
		/// as long as the state is active.
		/// </summary>
		public virtual void OnUpdateState()
		{

		}

		/// <summary>
		/// Called continuously from the fixed update loop of the state machine,
		/// as long as the state is active.
		/// </summary>
		public virtual void OnFixedUpdateState()
		{

		}

		/// <summary>
		/// Change the state of the state machine.
		/// </summary>
		/// <param name="state">The new state</param>
		public void ChangeState(T state)
		{
			if (StateMachine != null)
			{
				StateMachine.ChangeState(state);
			}
			else
			{
				Debug.LogWarning("Can't change state, because the StateMachine is not available");
			}
		}

		/// <summary>
		/// Change the state of the state machine to the state of this behaviour.
		/// </summary>
		public override void ChangeToThisState()
		{
			ChangeState(State);
		}
	}

	public abstract class AbstractStateBehaviourBase : MonoBehaviour
	{
		public abstract string StateName { get; }

		public abstract void ChangeToThisState();

		public abstract bool IsActive { get; }
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(AbstractStateBehaviourBase), true)]
	public class StateBehaviourEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (!(target is AbstractStateBehaviourBase stateBehaviour)) return;

			GUILayout.Space(10);

			if (!Application.isPlaying)
			{
				DrawStateLabel(stateBehaviour, false);
			}
			else
			{
				DrawStateLabel(stateBehaviour, stateBehaviour.IsActive);
				GUILayout.Space(10);
				DrawStateSwitchButton(stateBehaviour);

				GUI.enabled = !stateBehaviour.IsActive;
			}
		}

		private static void DrawStateLabel(AbstractStateBehaviourBase stateBehaviour, bool isActiveState)
		{
			var contentColor = GUI.contentColor;

			if (Application.isPlaying)
			{
				GUI.contentColor = isActiveState ? Color.green : new Color(1, 0.5f, 0);
			}

			GUILayout.Label($"Active on: {stateBehaviour.StateName}");
			GUI.contentColor = contentColor;
		}

		private static void DrawStateSwitchButton(AbstractStateBehaviourBase stateBehaviour)
		{
			if (GUILayout.Button("Switch to State", GUILayout.Height(20), GUILayout.Width(120)))
			{
				stateBehaviour.ChangeToThisState();
			}
		}

	}

#endif
}
