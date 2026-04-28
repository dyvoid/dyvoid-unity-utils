using System;
using UnityEngine;
using System.ComponentModel;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utility
{
	/// <summary>
	/// Base class for defining behaviors associated with specific states in a finite state machine (FSM).
	/// Must be attached to the same GameObject as the FSM component.
	/// </summary>
	/// <typeparam name="TState">The enum type representing states.</typeparam>
	/// <remarks>
	/// This class provides hooks for enter, exit, update, and fixed update methods,
	/// which can be overridden to implement state-specific logic.
	/// </remarks>
	/// <example>
	/// <code>
	/// public class JumpState : FSMStateBehaviour<PlayerState> {
	///     public override PlayerState State => PlayerState.Jump;
	///     
	///     public override void OnEnter(PlayerState prevState) {
	///         // Play jump animation
	///         // Apply initial jump force
	///     }
	///     
	///     public override void OnUpdateState() {
	///         // Handle jump logic
	///         if (IsGrounded()) {
	///             stateMachine.ChangeState(PlayerState.Idle);
	///         }
	///     }
	/// }
	/// </code>
	/// </example>
	public abstract class FSMStateBehaviour<TState> : StateBehaviourBase where TState : Enum
	{
		/// <summary>The state this behavior is associated with.</summary>
		public abstract TState State { get; }
		/// <summary>Gets the name of the state as a string.</summary>
		public override string StateName => State.ToString();

		private FSM<TState> __stateMachine;
		/// <summary>Gets the FSM component that's attached to the same GameObject.</summary>
		protected FSM<TState> stateMachine
		{
			get
			{
				// Lazy without using ??= due to issues with Unity's object lifecycle and serialization
				if (__stateMachine == null) __stateMachine = GetComponent<FSM<TState>>();
				return __stateMachine;
			}
		}

		/// <summary>
		/// Whether this behaviour is currently active
		/// </summary>
		public override bool IsActive
		{
			get
			{
				if (stateMachine != null)
				{
					return State.Equals(stateMachine.CurrentState);
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
		public virtual void OnEnter(TState prevState)
		{

		}

		/// <summary>
		/// Called once by the state machine, when the state becomes inactive.
		/// </summary>
		public virtual void OnExit(TState nextState)
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
		/// Convenience method to change the state of the state machine.
		/// </summary>
		/// <param name="state">The new state</param>
		public void ChangeState(TState state)
		{
			if (stateMachine != null)
			{
				stateMachine.ChangeState(state);
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

	/// <summary>
	/// Convenience class for state machines with a blackboard.
	/// Inherit from this when you want type-safe access to a specific Blackboard type.
	/// </summary>
	/// <typeparam name="TState">The enum type representing states</typeparam>
	/// <typeparam name="TBlackboard">The specific Blackboard type to use</typeparam>
	public abstract class FSMStateBehaviour<TState, TBlackboard> : FSMStateBehaviour<TState>
		where TState : Enum
		where TBlackboard : FSMBlackboard
	{
		/// <summary>
		/// Provides type-safe access to the state machine's blackboard.
		/// Will be null if the blackboard is not of type TBlackboard.
		/// </summary>
		protected TBlackboard blackboard => stateMachine.Blackboard as TBlackboard;
	}

	/// <summary>
	/// Non-generic base class required for Unity Editor integration of state behaviors.
	/// This is an implementation detail to support the custom editor UI.
	/// Do not inherit from this class, use either <see cref="FSMStateBehaviour{TState}"/> or
	/// <see cref="FSMStateBehaviour{TState, TBlackboard}"/> instead.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class StateBehaviourBase : MonoBehaviour
	{
		/// <summary>Gets the name of the state this behavior represents.</summary>
		public abstract string StateName { get; }

		/// <summary>Changes the state machine to this behavior's state.</summary>
		public abstract void ChangeToThisState();

		/// <summary>Gets whether this behavior's state is currently active.</summary>
		public abstract bool IsActive { get; }
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(StateBehaviourBase), true)]
	public class StateBehaviourEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (!(target is StateBehaviourBase stateBehaviour)) return;

			GUILayout.Space(10);

			if (!Application.isPlaying)
			{
				DrawStateLabel(stateBehaviour, false);
			}
			else
			{
				DrawStateLabel(stateBehaviour, stateBehaviour.IsActive);
				GUILayout.Space(10);

				using (new EditorGUI.DisabledScope(stateBehaviour.IsActive))
				{
					DrawStateSwitchButton(stateBehaviour);
				}
			}
		}

		private static void DrawStateLabel(StateBehaviourBase stateBehaviour, bool isActiveState)
		{
			var contentColor = GUI.contentColor;

			if (Application.isPlaying)
			{
				GUI.contentColor = isActiveState ? Color.green : new Color(1, 0.5f, 0);
			}

			GUILayout.Label($"Active on: {stateBehaviour.StateName}");
			GUI.contentColor = contentColor;
		}

		private static void DrawStateSwitchButton(StateBehaviourBase stateBehaviour)
		{
			if (GUILayout.Button("Switch to State", GUILayout.Height(20), GUILayout.Width(120)))
			{
				stateBehaviour.ChangeToThisState();
			}
		}

	}

#endif
}
