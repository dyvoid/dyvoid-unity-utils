using System;
using System.Collections.Generic;

namespace Utility
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
