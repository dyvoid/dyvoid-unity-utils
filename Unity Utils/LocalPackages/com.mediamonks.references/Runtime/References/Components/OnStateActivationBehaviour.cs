using UnityEngine;

namespace MediaMonks.References
{
	/// <summary>
	/// MonoBehaviour implementation of IOnStateActivation to make it possible to assign in the Unity inspector.
	/// </summary>
	public abstract class OnStateActivationBehaviour : MonoBehaviour, IOnStateActivation
	{
		public abstract void OnStateStartActivation(bool isActive);
		public abstract void OnStateActivation(bool isActive);
	}
}