using System;
using System.Collections.Generic;
using UnityEngine;

namespace MediaMonks.References
{
	/// <summary>
	/// Triggers assigned OnStateActivationBehaviour components when state changes.
	/// </summary>
	public abstract class StateActivationBehaviourTrigger<TEnum, TReferenceValue>
		: StateActivationTrigger<TEnum, TReferenceValue>, IOnStateActivation
		where TEnum : Enum where TReferenceValue : ReferenceValue<TEnum>
	{
		[Space]
		[Tooltip("The OnStateActivationBehaviour components that handles state changes.")]
		[SerializeField] private List<OnStateActivationBehaviour> _activationListeners;

		protected override IOnStateActivation StateActivationListener => this;

		public void OnStateStartActivation(bool isActive)
		{
			foreach (var listener in _activationListeners)
			{
				listener.OnStateStartActivation(isActive);
			}
		}

		public void OnStateActivation(bool isActive)
		{
			foreach (var listener in _activationListeners)
			{
				listener.OnStateActivation(isActive);
			}
		}

		protected void CollectAttachedComponents()
		{
			var components = GetComponents<OnStateActivationBehaviour>();
			if (components == null) return;

			foreach (var attachedBehaviour in components)
			{
				if (_activationListeners.IndexOf(attachedBehaviour) == -1)
				{
					_activationListeners.Add(attachedBehaviour);
				}
			}
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			// Prune empty entries
			for (var index = _activationListeners.Count - 1; index >= 0; index--)
			{
				if (_activationListeners[index] == null)
				{
					_activationListeners.RemoveAt(index);
				}
			}

			if (_activationListeners != null && _activationListeners.Count > 0) return;

			CollectAttachedComponents();
		}
#endif
	}
}