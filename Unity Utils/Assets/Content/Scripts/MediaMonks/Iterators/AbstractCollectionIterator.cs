using System;
using System.Collections.Generic;
using UnityEngine;

namespace MediaMonks.Iterators
{
	public abstract class AbstractCollectionIterator<T> : MonoBehaviour
	{
		protected IList<T> List;

		protected AbstractCollectionIterator(IList<T> list)
		{
			List = list;
		}

		public abstract T Next();

	}
}