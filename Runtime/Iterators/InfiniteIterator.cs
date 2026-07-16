using System;
using System.Collections.Generic;

namespace dyvoid.Utility
{
	public enum IterationMode
	{
		Sequential,
		PingPong,
		Random
	}

	/// <summary>
	/// Infinitely iterates over an IList using a specified traversal strategy.
	/// </summary>
	public class InfiniteIterator<T> : CollectionIteratorBase<T>
	{
		private readonly ITraversalStrategy strategy;

		public InfiniteIterator(IList<T> list, IterationMode mode = IterationMode.Sequential) : base(list)
		{
			strategy = mode switch
			{
				IterationMode.Sequential => new SequentialStrategy(),
				IterationMode.PingPong => new PingPongStrategy(),
				IterationMode.Random => new RandomStrategy(),
				_ => throw new ArgumentOutOfRangeException(nameof(mode))
			};
		}

		/// <summary>
		/// Creates an infinite iterator with a custom traversal strategy.
		/// </summary>
		/// <param name="list">The collection to iterate over</param>
		/// <param name="strategy">The traversal strategy that determines the iteration order.
		/// Strategies are stateful; do not share one instance between iterators.</param>
		public InfiniteIterator(IList<T> list, ITraversalStrategy strategy) : base(list)
		{
			this.strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
		}

		public override T Next()
		{
			if (list.Count == 0)
			{
				throw new InvalidOperationException("Cannot iterate over an empty collection");
			}

			int index = strategy.GetNextIndex(list.Count);
			return list[index];
		}
	}
}
