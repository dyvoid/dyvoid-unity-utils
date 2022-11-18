using System;
using System.Collections.Generic;
using Temple.Iterators;
using UnityEngine;
using Random = System.Random;

namespace Temple.Iterators
{
	/// <summary>
	/// Infinitely iterates over an IList. 
	/// </summary>
	public class InfiniteIterator<T> : AbstractCollectionIterator<T>
	{
		private int _index = 0;

		public InfiniteIterator(IList<T> list) : base(list)
		{
		}

		public override T Next()
		{
			int collectionSize = List.Count;

			if (collectionSize == 0) throw new Exception("Cannot iterate over an empty collection");

			// Clamp the index, in case the collection has been mutated
			_index = Mathf.Clamp(_index, 0, collectionSize - 1);

			T item = List[_index];

			// Increase the index, and wrap it if we've reached the end
			_index = (_index + 1) % collectionSize;

			return item;
		}
	}
}
