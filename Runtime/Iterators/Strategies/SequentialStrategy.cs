using UnityEngine;

namespace dyvoid.Utility
{
	/// <summary>
	/// Traverses sequentially: 0, 1, 2, ..., N, 0, 1, 2, ...
	/// </summary>
	internal class SequentialStrategy : ITraversalStrategy
	{
		private int index = 0;

		public int GetNextIndex(int collectionSize)
		{
			// Clamp the index in case the collection has been mutated.
			index = Mathf.Clamp(index, 0, collectionSize - 1);

			int currentIndex = index;

			index = (index + 1) % collectionSize;

			return currentIndex;
		}
	}
}
