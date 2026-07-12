using System;
using System.Collections.Generic;

namespace dyvoid.Utility
{
	/// <summary>
	/// Infinitely iterates over a collection, returning only items that pass a predicate.
	/// Useful for iterating over e.g. only the alive enemies or active pooled objects in a list.
	/// </summary>
	/// <remarks>
	/// Items that fail the predicate are skipped but still advance the underlying traversal,
	/// so e.g. sequential order is preserved across the skipped items. If no item passes the
	/// predicate within <c>2 * list.Count</c> attempts, <see cref="Next"/> throws; use
	/// <see cref="TryNext"/> when an empty result is an expected state.
	/// </remarks>
	public class FilteredIterator<T> : CollectionIteratorBase<T>
	{
		private readonly InfiniteIterator<T> innerIterator;
		private readonly Func<T, bool> predicate;

		/// <summary>
		/// Creates a filtered iterator.
		/// </summary>
		/// <param name="list">The collection to iterate over</param>
		/// <param name="predicate">Only items for which this returns true are returned</param>
		/// <param name="mode">The traversal mode</param>
		public FilteredIterator(IList<T> list, Func<T, bool> predicate, IterationMode mode = IterationMode.Sequential)
			: this(list, predicate, new InfiniteIterator<T>(list, mode))
		{
		}

		/// <summary>
		/// Creates a filtered iterator with a custom traversal strategy.
		/// </summary>
		/// <param name="list">The collection to iterate over</param>
		/// <param name="predicate">Only items for which this returns true are returned</param>
		/// <param name="strategy">The traversal strategy that determines the iteration order.
		/// Strategies are stateful; do not share one instance between iterators.</param>
		public FilteredIterator(IList<T> list, Func<T, bool> predicate, ITraversalStrategy strategy)
			: this(list, predicate, new InfiniteIterator<T>(list, strategy))
		{
		}

		private FilteredIterator(IList<T> list, Func<T, bool> predicate, InfiniteIterator<T> innerIterator) : base(list)
		{
			this.predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
			this.innerIterator = innerIterator;
		}

		/// <summary>
		/// Returns the next item that passes the predicate.
		/// </summary>
		/// <returns>The next matching item</returns>
		/// <exception cref="InvalidOperationException">Thrown when the collection is empty or
		/// no item passes the predicate within <c>2 * list.Count</c> attempts.</exception>
		public override T Next()
		{
			if (list.Count == 0)
			{
				throw new InvalidOperationException("Cannot iterate over an empty collection");
			}

			if (TryNext(out T item))
			{
				return item;
			}

			throw new InvalidOperationException("No items in the collection pass the predicate");
		}

		/// <summary>
		/// Attempts to return the next item that passes the predicate, without throwing
		/// when no item currently matches.
		/// </summary>
		/// <param name="item">The next matching item, or default if none was found</param>
		/// <returns>True if a matching item was found</returns>
		public bool TryNext(out T item)
		{
			// Two full traversals is enough for every strategy to have visited each
			// item at least once (PingPong revisits interior items on the way back).
			int maxAttempts = 2 * list.Count;

			for (int attempt = 0; attempt < maxAttempts; attempt++)
			{
				T candidate = innerIterator.Next();

				if (predicate(candidate))
				{
					item = candidate;
					return true;
				}
			}

			item = default;
			return false;
		}
	}
}
