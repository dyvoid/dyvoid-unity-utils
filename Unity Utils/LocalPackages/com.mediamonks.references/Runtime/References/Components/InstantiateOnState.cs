using System;
using UnityEngine;

namespace MediaMonks.References
{
	/// <summary>
	/// Instantiates a prefab when state conditions are met. Does not re-instantiate when instance is still present when conditions are met again.
	/// </summary>
	/// <typeparam name="TEnum">The enum type with state definitions.</typeparam>
	/// <typeparam name="TReferenceValue">The ReferenceValue type used for accessing the state.</typeparam>
	public abstract class InstantiateOnState<TEnum, TReferenceValue> : StateActivationTrigger<TEnum, TReferenceValue>, IOnStateActivation
		where TEnum : Enum where TReferenceValue : ReferenceValue<TEnum>
	{
		[Space]
		[SerializeField] private GameObject _prefab;
		[SerializeField] private Transform _parent;

		private GameObject _instance;

		public void OnStateStartActivation(bool isActive)
		{
			OnStateActivation(isActive);
		}

		public void OnStateActivation(bool isActive)
		{
			if (!isActive) return;

			if (_instance != null) return;

			_instance = Instantiate(_prefab, _parent);
		}
		
		protected override IOnStateActivation StateActivationListener => this;
	}
}