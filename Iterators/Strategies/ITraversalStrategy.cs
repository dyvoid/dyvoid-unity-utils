namespace Utility
{
	/// <summary>
	/// Defines how to traverse a collection by providing the next index.
	/// </summary>
	internal interface ITraversalStrategy
	{
		/// <summary>
		/// Returns the next index to access in the collection.
		/// </summary>
		/// <param name="collectionSize">Current size of the collection.</param>
		/// <returns>The next index.</returns>
		int GetNextIndex(int collectionSize);
	}
}
