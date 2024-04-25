using System;
using System.Collections.Generic;

namespace MediaMonks.Iterators
{
	public abstract class AbstractCollectionIterator<T>
	{
		protected IList<T> List;

		protected AbstractCollectionIterator(IList<T> list)
		{
			List = list;
		}

		public abstract T Next();

	}
}