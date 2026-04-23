using System;
using System.Collections.Generic;

namespace Iterators
{
	public abstract class CollectionIteratorBase<T>
	{
		protected IList<T> list;

		protected CollectionIteratorBase(IList<T> list)
		{
			this.list = list;
		}

		public abstract T Next();

	}
}
