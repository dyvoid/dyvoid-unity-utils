using System;
using System.Collections.Generic;

namespace Iterators
{
	/// <summary>
	/// Chains multiple collections into a single iteration sequence.
	/// Iterates through each collection in order.
	/// </summary>
	public class CombinedIterator<T>
	{
		private readonly List<CollectionIteratorBase<T>> iterators = new();
		private int currentIteratorIndex = 0;

		/// <summary>
		/// Creates a combined iterator from multiple collections
		/// </summary>
		/// <param name="collections">Collections to iterate through in sequence</param>
		public CombinedIterator(params IList<T>[] collections)
		{
			if (collections.Length == 0)
			{
				throw new ArgumentException("At least one collection must be provided");
			}

			foreach (var collection in collections)
			{
				if (collection.Count > 0)
				{
					this.iterators.Add(new InfiniteIterator<T>(collection));
				}
			}

			if (this.iterators.Count == 0)
			{
				throw new ArgumentException("All provided collections are empty");
			}
		}

		/// <summary>
		/// Creates a combined iterator from multiple iterators
		/// </summary>
		/// <param name="iterators">Iterators to use in sequence</param>
		public CombinedIterator(params CollectionIteratorBase<T>[] iterators)
		{
			if (iterators.Length == 0)
			{
				throw new ArgumentException("At least one iterator must be provided");
			}

			foreach (var iterator in iterators)
			{
				if (iterator == null)
				{
					throw new ArgumentException("Iterators must not be null", nameof(iterators));
				}

				this.iterators.Add(iterator);
			}
		}

		/// <summary>
		/// Returns the next item in the sequence, moving to the next collection when needed
		/// </summary>
		/// <returns>The next item</returns>
		public T Next()
		{
			if (iterators.Count == 0)
			{
				throw new InvalidOperationException("No valid iterators available");
			}

			return iterators[currentIteratorIndex].Next();
		}

		/// <summary>
		/// Advances to the next collection in the sequence
		/// </summary>
		public void NextCollection()
		{
			if (iterators.Count > 1)
			{
				currentIteratorIndex = (currentIteratorIndex + 1) % iterators.Count;
			}
		}

		/// <summary>
		/// Sets the active collection by index
		/// </summary>
		/// <param name="index">Index of the collection to make active</param>
		public void SetCollection(int index)
		{
			if (index < 0 || index >= iterators.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			currentIteratorIndex = index;
		}
	}
}
