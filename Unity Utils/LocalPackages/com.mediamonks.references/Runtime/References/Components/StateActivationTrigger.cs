using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace MediaMonks.References
{
	/// <summary>
	/// Non generic base class for generic StateActivationTrigger classes.
	/// </summary>
	public abstract class StateActivationTriggerBase : MonoBehaviour
	{
		public abstract bool AnyStateActive { get; }
		public abstract ScriptableObjectReferenceBase ReferenceValue { get; }
	}

	/// <summary>
	/// Sets the a Activation based on the state.
	/// </summary>
	/// <typeparam name="TEnum">The enum type with state definitions.</typeparam>
	/// <typeparam name="TReferenceValue">The ReferenceValue type used for accessing the state.</typeparam>
	public abstract class StateActivationTrigger<TEnum, TReferenceValue> : StateActivationTriggerBase
		where TEnum : Enum where TReferenceValue : ReferenceValue<TEnum>
	{
		[SerializeField] private TReferenceValue _stateValue;
		[SerializeField] private TEnum _activeStates;
		[SerializeField] private AtStart _atStart = AtStart.CheckState;

		public TEnum State => _stateValue.Value;
		public override ScriptableObjectReferenceBase ReferenceValue => _stateValue;

		public override bool AnyStateActive => IsStateSet(_stateValue.Value);

		private enum AtStart
		{
			Show,
			Hide,
			CheckState
		}

		protected abstract IOnStateActivation StateActivationListener { get; }

		private void Start()
		{
			if (_stateValue == null)
			{
				throw new NullReferenceException("State Value has not been set.");
			}

			var setter = StateActivationListener ??
			             throw new NullReferenceException($"No {nameof(IOnStateActivation)} available for {name}");

			switch (_atStart)
			{
				case AtStart.Show:
					setter.OnStateStartActivation(true);
					break;
				case AtStart.Hide:
					setter.OnStateStartActivation(false);
					break;
				case AtStart.CheckState:
					setter.OnStateStartActivation(IsStateSet(_stateValue.Value));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			_stateValue.ValueHasChanged += OnStateHasChanged;
		}

		private void OnDestroy()
		{
			_stateValue.ValueHasChanged -= OnStateHasChanged;
		}

		private void OnStateHasChanged(TEnum newValue, TEnum oldValue)
		{
			StateActivationListener.OnStateActivation(IsStateSet(newValue));
		}

		private bool IsStateSet(TEnum state)
		{
			return ((int) (object) state & (int) (object) _activeStates) != 0;
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(StateActivationTriggerBase), true)]
	public class ShowOnEnumStateEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (!Application.isPlaying)
			{
				return;
			}

			if (!(target is StateActivationTriggerBase stateHandler)) return;

			if (stateHandler.ReferenceValue == null) return;

			var contentColor = GUI.contentColor;
			GUI.contentColor = stateHandler.AnyStateActive ? Color.green : new Color(1, 0.5f, 0);

			if (!stateHandler.ReferenceValue.TryGetValue(out Enum state)) return;

			GUILayout.Space(10);
			GUILayout.Label($"Current state: {state}");

			GUI.contentColor = contentColor;
		}
	}
#endif
}