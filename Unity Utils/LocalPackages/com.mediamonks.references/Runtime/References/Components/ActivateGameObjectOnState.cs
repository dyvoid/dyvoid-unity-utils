using System;

namespace MediaMonks.References
{
	/// <summary>
	/// Sets the a GameObject active based on the state.
	/// </summary>
	/// <typeparam name="TEnum">The enum type with state definitions.</typeparam>
	/// <typeparam name="TReferenceValue">The ReferenceValue type used for accessing the state.</typeparam>
	public abstract class ActivateGameObjectOnState<TEnum, TReferenceValue>
		: StateActivationTrigger<TEnum, TReferenceValue>, IOnStateActivation
		where TEnum : Enum where TReferenceValue : ReferenceValue<TEnum>
	{
		public void OnStateStartActivation(bool isActive)
		{
			gameObject.SetActive(isActive);
		}

		public void OnStateActivation(bool isActive)
		{
			gameObject.SetActive(isActive);
		}

		protected override IOnStateActivation StateActivationListener => this;
	}
}