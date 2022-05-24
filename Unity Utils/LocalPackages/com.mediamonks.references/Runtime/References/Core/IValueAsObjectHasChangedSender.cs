namespace MediaMonks.References
{
	/// <summary>
	/// Interface for notifying a value change where value is cast to an object.
	/// </summary>
	public interface IValueAsObjectHasChangedSender
	{
		delegate void ValueAsObjectHasChangedDelegate(object newValue, object oldValue);

		event ValueAsObjectHasChangedDelegate ValueAsObjectHasChanged;
	}
}