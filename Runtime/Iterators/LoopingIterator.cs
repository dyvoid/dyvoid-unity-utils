using System;
using System.Collections.Generic;

namespace dyvoid.Utility
{
	/// <summary>
	/// Iterates over a collection a fixed number of times, then stops.
	/// One loop is completed after <c>list.Count</c> items have been returned.
	/// </summary>
	/// <remarks>
	/// This is the finite counterpart to <see cref="InfiniteIterator{T}"/>: once
	/// <see cref="IsDone"/> is true, <see cref="Next"/> throws and the enumerator ends,
	/// so it can be safely consumed with <c>foreach</c> or LINQ without <c>Take(n)</c>.
	/// Create a new instance to run the loops again.
	/// Note that for <see cref="IterationMode.PingPong"/> a "loop" still means
	/// <c>list.Count</c> returned items, not a full forward-and-back sweep.
	/// </remarks>
	public class LoopingIterator<T> : CollectionIteratorBase<T>
	{
		private readonly InfiniteIterator<T> innerIterator;
		private int itemsReturnedInLoop;

		/// <summary>The total number of loops this iterator performs.</summary>
		public int LoopCount { get; }

		/// <summary>The number of loops completed so far.</summary>
		public int LoopsCompleted { get; private set; }

		/// <summary>Whether all loops have been completed.</summary>
		public bool IsDone => LoopsCompleted >= LoopCount;

		/// <summary>
		/// Creates a looping iterator.
		/// </summary>
		/// <param name="list">The collection to iterate over</param>
		/// <param name="loopCount">How many times to iterate over the collection</param>
		/// <param name="mode">The traversal mode</param>
		public LoopingIterator(IList<T> list, int loopCount, IterationMode mode = IterationMode.Sequential)
			: this(list, loopCount, new InfiniteIterator<T>(list, mode))
		{
		}

		/// <summary>
		/// Creates a looping iterator with a custom traversal strategy.
		/// </summary>
		/// <param name="list">The collection to iterate over</param>
		/// <param name="loopCount">How many times to iterate over the collection</param>
		/// <param name="strategy">The traversal strategy that determines the iteration order.
		/// Strategies are stateful; do not share one instance between iterators.</param>
		public LoopingIterator(IList<T> list, int loopCount, ITraversalStrategy strategy)
			: this(list, loopCount, new InfiniteIterator<T>(list, strategy))
		{
		}

		private LoopingIterator(IList<T> list, int loopCount, InfiniteIterator<T> innerIterator) : base(list)
		{
			if (loopCount <= 0)
			{
				throw new ArgumentException("Loop count must be greater than zero", nameof(loopCount));
			}

			LoopCount = loopCount;
			this.innerIterator = innerIterator;
		}

		/// <summary>
		/// Returns the next item.
		/// </summary>
		/// <returns>The next item</returns>
		/// <exception cref="InvalidOperationException">Thrown when the collection is empty
		/// or all loops have been completed (<see cref="IsDone"/>).</exception>
		public override T Next()
		{
			if (IsDone)
			{
				throw new InvalidOperationException(
					"All loops have been completed; create a new LoopingIterator to iterate again");
			}

			T item = innerIterator.Next();

			itemsReturnedInLoop++;
			if (itemsReturnedInLoop >= list.Count)
			{
				itemsReturnedInLoop = 0;
				LoopsCompleted++;
			}

			return item;
		}

		/// <summary>
		/// Enumerates the remaining items and terminates when all loops are completed.
		/// </summary>
		public override IEnumerator<T> GetEnumerator()
		{
			while (!IsDone)
			{
				yield return Next();
			}
		}
	}
}
