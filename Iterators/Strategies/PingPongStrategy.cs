using UnityEngine;

namespace Utility
{
	/// <summary>
	/// Traverses forward then backward: 0, 1, 2, 3, 2, 1, 0, 1, 2, ...
	/// </summary>
	internal class PingPongStrategy : ITraversalStrategy
	{
		private int index = 0;
		private bool isMovingForward = true;

		public int GetNextIndex(int collectionSize)
		{
			if (collectionSize == 1)
			{
				return 0;
			}

			// Clamp the index in case the collection has been mutated.
			index = Mathf.Clamp(index, 0, collectionSize - 1);

			int currentIndex = index;

			// Update direction if we've reached an end.
			if (index == collectionSize - 1 && isMovingForward)
			{
				isMovingForward = false;
			}
			else if (index == 0 && !isMovingForward)
			{
				isMovingForward = true;
			}

			index += isMovingForward ? 1 : -1;

			return currentIndex;
		}
	}
}
