using System;
using System.Collections.Generic;

namespace Temple.Utils
{
	/// <summary>
	/// Infinitely iterates randomly over a List or an Array. The same element will never be
	/// returned twice in a row. Every element of the collection will be returned once, before
	/// the full collection is randomized again.
	/// Note that if the collection is mutated externally, it cannot be guaranteed that the
	/// same element will not be returned twice in a row.
	/// </summary>
	public class RandomIterator<T>
	{
		private List<T> _list;
		private T[] _array;

		private List<int> _indices = new();
		private int _indicesIndex;

		private CollectionType _collectionType;

		public RandomIterator(List<T> list)
		{
			_list = list;

			_collectionType = CollectionType.List;
		}

		public RandomIterator(T[] array)
		{
			_array = array;

			_collectionType = CollectionType.Array;
		}

		public T Next()
		{
			int collectionSize = GetCollectionSize();

			if (collectionSize == 0) throw new Exception("Cannot iterate over an empty collection");
			if (collectionSize == 1) return _list[0];

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
					// return, remove and add it back as the last element of the indices list
					if (_indices[0] == nextIndex)
					{
						_indices.RemoveAt(0);
						_indices.Add(nextIndex);
					}
				}
			}

			return (_collectionType) switch
			{
				CollectionType.List => _list[nextIndex],
				CollectionType.Array => _array[nextIndex],
				_ => throw new ArgumentOutOfRangeException(nameof(_collectionType), _collectionType, null),
			};
		}

		/// <summary>
		/// Creates a random list containing all of the indices of the list/array
		/// we're iterating on.
		/// </summary>
		private void CreateRandomIndicesList()
		{
			_indices.Clear();
			_indicesIndex = 0;

			var numIndices = GetCollectionSize();
			for (int i = 0; i < numIndices; i++)
			{
				_indices.Add(i);
			}

			Shuffle(_indices);
		}

		private int GetCollectionSize()
		{
			return (_collectionType) switch
			{
				CollectionType.List => _list.Count,
				CollectionType.Array => _array.Length,
				_ => throw new ArgumentOutOfRangeException(nameof(_collectionType), _collectionType, null),
			};
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

		enum CollectionType
		{
			List,
			Array,
		}
	}
}
