using System;
using System.Collections.Generic;

namespace Iterators
{
	/// <summary>
	/// Returns items in groups/batches rather than individually.
	/// Useful for batch processing or chunking data.
	/// </summary>
	public class GroupedIterator<T>
	{
		private readonly CollectionIteratorBase<T> baseIterator;
		private readonly int groupSize;

		/// <summary>
		/// Creates a grouped iterator that wraps another iterator.
		/// </summary>
		/// <param name="baseIterator">The iterator to group items from</param>
		/// <param name="groupSize">Number of items to return in each group</param>
		public GroupedIterator(CollectionIteratorBase<T> baseIterator, int groupSize)
		{
			if (groupSize <= 0)
			{
				throw new ArgumentException("Group size must be greater than zero", nameof(groupSize));
			}

			this.baseIterator = baseIterator;
			this.groupSize = groupSize;
		}

		/// <summary>
		/// Creates a grouped iterator that operates directly on a collection.
		/// </summary>
		/// <param name="list">The collection to iterate over</param>
		/// <param name="groupSize">Number of items to return in each group</param>
		public GroupedIterator(IList<T> list, int groupSize)
		{
			if (groupSize <= 0)
			{
				throw new ArgumentException("Group size must be greater than zero", nameof(groupSize));
			}

			this.baseIterator = new InfiniteIterator<T>(list);
			this.groupSize = groupSize;
		}

		/// <summary>
		/// Returns the next group of items.
		/// </summary>
		/// <returns>A list containing the next batch of items</returns>
		public List<T> NextGroup()
		{
			var group = new List<T>(groupSize);

			for (int i = 0; i < groupSize; i++)
			{
				group.Add(baseIterator.Next());
			}

			return group;
		}

		/// <summary>
		/// Returns the next group of items with a potentially variable size.
		/// </summary>
		/// <param name="count">Number of items to return in this specific group</param>
		/// <returns>A list containing the next batch of items</returns>
		public List<T> NextGroup(int count)
		{
			if (count <= 0)
			{
				throw new ArgumentException("Count must be greater than zero", nameof(count));
			}

			var group = new List<T>(count);

			for (int i = 0; i < count; i++)
			{
				group.Add(baseIterator.Next());
			}

			return group;
		}
	}
}
