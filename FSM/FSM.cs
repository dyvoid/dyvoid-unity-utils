using System;
using System.Collections.Generic;
using Utility;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace State
{
	/// <summary>
	/// A flexible, component-based finite state machine implementation that manages state transitions
	/// and associated behaviors. This FSM is designed to work with Unity's component system and MonoBehaviour lifecycle.
	/// </summary>
	/// <typeparam name="TState">The enum type that defines all possible states of the FSM</typeparam>
	/// <example>
	/// <code>
	/// // 1. Define your states
	/// public enum PlayerState { Idle, Walk, Jump }
	/// 
	/// // 2. Create your FSM class
	/// public class PlayerFSM : FSM<PlayerState> { }
	/// 
	/// // 3. Create state behaviors (one per state)
	/// public class IdleState : FSMStateBehaviour<PlayerState> {
	///     public override PlayerState State => PlayerState.Idle;
	///     public override void OnEnter(PlayerState prevState) { /* setup */ }
	///     public override void OnUpdateState() { /* update logic */ }
	/// }
	/// </code>
	/// </example>
	public abstract class FSM<TState> : MonoBehaviour where TState : Enum
	{
		[SerializeField] private bool logStateChanges = false;

		/// <summary>Observable that emits state changes.</summary>
		public ObservableValue<TState> StateObservable { get; } = new();

		/// <summary>Currently active state behaviour.</summary>
		public FSMStateBehaviour<TState> CurrentBehaviour { get; private set; }
		/// <summary>Previously active state behaviour.</summary>
		public FSMStateBehaviour<TState> PreviousBehaviour { get; private set; }

		/// <summary>Currently active state.</summary>
		public TState CurrentState => StateObservable.Value;
		/// <summary>Previously active state, or default if no previous state exists.</summary>
		public TState PreviousState => (PreviousBehaviour != null) ? PreviousBehaviour.State : default(TState);

		private readonly Dictionary<TState, FSMStateBehaviour<TState>> behaviourMap = new Dictionary<TState, FSMStateBehaviour<TState>>();        
		
		/// <summary>
		/// Gets the FSMBlackboard component attached to the same GameObject.
		/// </summary>
		private FSMBlackboard blackboard;
		public FSMBlackboard Blackboard
		{
			get
			{
				// Lazy without using ??= due to issues with Unity's object lifecycle and serialization
				if (blackboard == null) blackboard = GetComponent<FSMBlackboard>();                
				return blackboard;
			}
		}

		/// <summary>Initializes the FSM by gathering all state behaviours and setting up state change handling.</summary>
		protected virtual void Start()
		{
			// Fetch all the state behaviours on this game object
			foreach (FSMStateBehaviour<TState> behavior in gameObject.GetComponents<FSMStateBehaviour<TState>>())
			{
				behaviourMap.Add(behavior.State, behavior);
			}

			// Init the observable
			StateObservable.Subscribe(HandleStateChanged, true);
		}

		/// <summary>
		/// Get a behavior that is registered with this state machine.
		/// </summary>
		/// <param name="state">The state of the behaviour to get</param>
		/// <returns>The state behaviour</returns>
		public FSMStateBehaviour<TState> GetBehavior(TState state)
		{
			return behaviourMap[state];
		}

		/// <summary>
		/// Change to a new state.
		/// </summary>
		/// <param name="newState">The state to change to</param>
		public void ChangeState(TState newState)
		{
			StateObservable.Value = newState;
		}

		protected virtual void HandleStateChanged(TState newState, TState oldState)
		{
			if (logStateChanges) Debug.Log($"[State Machine] State changed: {newState}");

			// First exit the current state behaviour, if one is active
			CurrentBehaviour?.OnExit(newState);

			PreviousBehaviour = CurrentBehaviour;

			// Now attempt to enter the new state behaviour, if it is available
			if (behaviourMap.TryGetValue(newState, out var newStateBehaviour))
			{
				CurrentBehaviour = newStateBehaviour;
				CurrentBehaviour.OnEnter(oldState);
			}
			else
			{
				CurrentBehaviour = null;
			}
		}

		/// <summary>Updates the current state behaviour each frame.</summary>
		protected virtual void Update()
		{
			if (CurrentBehaviour != null) CurrentBehaviour.OnUpdateState();
		}

		/// <summary>Updates the current state behaviour at fixed time intervals.</summary>
		protected virtual void FixedUpdate()
		{
			if (CurrentBehaviour != null) CurrentBehaviour.OnFixedUpdateState();
		}

		/// <summary>Cleans up subscriptions when the FSM is destroyed.</summary>
		protected virtual void OnDestroy()
		{
			StateObservable.Unsubscribe(HandleStateChanged);
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(FSM<>), true)]
	public class StateMachineEditor : Editor
	{
		private int selectedStateIndex = 0;
		private string[] stateNames;
		private Array enumValues;

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (!Application.isPlaying) return;

			var stateMachine = target as MonoBehaviour;
			if (stateMachine == null) return;

			// Use reflection to get the generic type parameter (the enum type)
			var baseType = stateMachine.GetType().BaseType;
			while (baseType != null && (!baseType.IsGenericType || baseType.GetGenericTypeDefinition() != typeof(FSM<>)))
			{
				baseType = baseType.BaseType;
			}

			if (baseType == null) return;

			var enumType = baseType.GetGenericArguments()[0];
			
			// Get all enum values if we haven't already
			if (enumValues == null)
			{
				enumValues = Enum.GetValues(enumType);
				stateNames = Enum.GetNames(enumType);
				
				// Find the current state in the enum values
				var stateProperty = stateMachine.GetType().GetProperty("CurrentState");
				if (stateProperty != null)
				{
					var currentState = stateProperty.GetValue(stateMachine);
					for (int i = 0; i < enumValues.Length; i++)
					{
						if (enumValues.GetValue(i).Equals(currentState))
						{
							selectedStateIndex = i;
							break;
						}
					}
				}
			}

			EditorGUILayout.Space(10);
			EditorGUILayout.LabelField("State Machine Controls", EditorStyles.boldLabel);
			
			// Show current state
			var statePropertyInfo = stateMachine.GetType().GetProperty("CurrentState");
			if (statePropertyInfo != null)
			{
				var currentStateValue = statePropertyInfo.GetValue(stateMachine);
				EditorGUILayout.LabelField("Current State:", currentStateValue.ToString());
			}
			
			EditorGUILayout.Space(5);
			
			// State selection dropdown
			EditorGUILayout.BeginHorizontal();
			selectedStateIndex = EditorGUILayout.Popup("Change State:", selectedStateIndex, stateNames);
			
			// Change state button
			if (GUILayout.Button("Switch", GUILayout.Width(60)))
			{
				var changeStateMethod = stateMachine.GetType().GetMethod("ChangeState");
				if (changeStateMethod != null)
				{
					var newState = enumValues.GetValue(selectedStateIndex);
					changeStateMethod.Invoke(stateMachine, new[] { newState });
				}
			}
			EditorGUILayout.EndHorizontal();
		}
	}
#endif
}
