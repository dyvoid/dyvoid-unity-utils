using System;
using System.Collections.Generic;

namespace Iterators
{
	/// <summary>
	/// Traverses in random order. Every index is returned once before reshuffling.
	/// The same index will never be returned twice in a row.
	/// </summary>
	internal class RandomStrategy : ITraversalStrategy
	{
		private readonly List<int> indices = new();
		private int indicesIndex;

		public int GetNextIndex(int collectionSize)
		{
			if (collectionSize == 1)
			{
				return 0;
			}

			// Create new indices list if it doesn't exist yet, or if the collection size has changed.
			if (collectionSize != indices.Count)
			{
				CreateRandomIndicesList(collectionSize);
			}

			int nextIndex = indices[indicesIndex];
			indicesIndex++;

			// If we've reached the end of our indices list, reset and reshuffle.
			if (indicesIndex == indices.Count)
			{
				indicesIndex = 0;

				if (indices.Count > 2)
				{
					Shuffle(indices);

					// If the new first element is the same as the index we're about to
					// return, swap it with the last element to avoid repeats.
					if (indices[0] == nextIndex)
					{
						(indices[0], indices[^1]) = (indices[^1], indices[0]);
					}
				}
			}

			return nextIndex;
		}

		private void CreateRandomIndicesList(int collectionSize)
		{
			indices.Clear();
			indicesIndex = 0;

			for (int i = 0; i < collectionSize; i++)
			{
				indices.Add(i);
			}

			Shuffle(indices);
		}

		private void Shuffle(IList<int> list)
		{
			if (list == null)
			{
				throw new ArgumentNullException(nameof(list));
			}

			int n = list.Count;

			while (n > 1)
			{
				n--;
				int k = UnityEngine.Random.Range(0, n + 1);
				(list[n], list[k]) = (list[k], list[n]);
			}
		}
	}
}
