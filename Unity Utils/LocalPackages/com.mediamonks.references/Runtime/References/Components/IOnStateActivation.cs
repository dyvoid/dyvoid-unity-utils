namespace MediaMonks.References
{
	/// <summary>
	/// Interface for handling state activation changes.
	/// </summary>
	public interface IOnStateActivation
	{
		/// <summary>
		/// Sets the start Activation.
		/// </summary>
		void OnStateStartActivation(bool isActive);

		/// <summary>
		/// Sets the Activation.
		/// </summary>
		void OnStateActivation(bool isActive);
	}
}