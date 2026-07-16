namespace dyvoid.Utility
{
	/// <summary>
	/// Defines how to traverse a collection by providing the next index.
	/// Implement this to plug a custom traversal order into <see cref="InfiniteIterator{T}"/>.
	/// </summary>
	/// <remarks>
	/// Implementations are typically stateful (they track their position between calls),
	/// so a strategy instance should not be shared between iterators.
	/// <see cref="GetNextIndex"/> receives the collection's current size on every call and
	/// must handle the size changing between calls.
	/// </remarks>
	public interface ITraversalStrategy
	{
		/// <summary>
		/// Returns the next index to access in the collection.
		/// </summary>
		/// <param name="collectionSize">Current size of the collection.</param>
		/// <returns>The next index.</returns>
		int GetNextIndex(int collectionSize);
	}
}
