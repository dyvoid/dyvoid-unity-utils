using System;
using System.Collections.Generic;
using Temple.Iterators;

namespace Temple.Iterators
{
	/// <summary>
	/// Infinitely iterates randomly over a List or an Array. The same element will never be
	/// returned twice in a row. Every element of the collection will be returned once, before
	/// the full collection is randomized again.
	/// Note that if the collection is mutated externally, it cannot be guaranteed that the
	/// same element will not be returned twice in a row.
	/// </summary>
	public class RandomIterator<T> : AbstractCollectionIterator<T>
	{
		private List<int> _indices = new();
		private int _indicesIndex;


		public RandomIterator(IList<T> list) : base(list)
		{
		}

		public override T Next()
		{
			int collectionSize = List.Count;

			if (collectionSize == 0) throw new Exception("Cannot iterate over an empty collection");
			if (collectionSize == 1) return List[0];

			// Create new indices list if it doesn't exist yet, or if the collection size has changed
			if (_indices == null || collectionSize != _indices.Count)
			{
				CreateRandomIndicesList();
			}

			int nextIndex = _indices[_indicesIndex];
			_indicesIndex++;

			// If we've reached the end of our indices list, reset our index.
			// If the list has more than 2 items, reshuffle it.
			if (_indicesIndex == _indices.Count)
			{
				_indicesIndex = 0;

				if (_indices.Count > 2)
				{
					Shuffle(_indices);

					// If the new first element is the same as the index we're about to
					// return, swap it with the last element of the indices list
					if (_indices[0] == nextIndex)
					{
						(_indices[0], _indices[^1]) = (_indices[^1], _indices[0]);
					}
				}
			}

			return List[nextIndex];
		}

		/// <summary>
		/// Creates a random list containing all of the indices of the list/array
		/// we're iterating on.
		/// </summary>
		private void CreateRandomIndicesList()
		{
			_indices.Clear();
			_indicesIndex = 0;

			var numIndices = List.Count;
			for (int i = 0; i < numIndices; i++)
			{
				_indices.Add(i);
			}

			Shuffle(_indices);
		}
		
		/// <summary>
		/// Shuffles the contents of the <paramref name="list"/>
		/// </summary>
		private void Shuffle<T>(IList<T> list, Random random = null)
		{
			if (list == null) throw new ArgumentNullException(nameof(list));

			var rnd = random ?? new Random(DateTime.Now.Millisecond);
			var n = list.Count;

			while (n > 1)
			{
				n--;
				int k = rnd.Next(n + 1);
				(list[n], list[k]) = (list[k], list[n]);
			}
		}
	}
}
