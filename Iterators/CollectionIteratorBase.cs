using System.Collections;
using System.Collections.Generic;

namespace Iterators
{
	/// <summary>
	/// Base class for collection iterators.
	/// Implements <see cref="IEnumerable{T}"/> so iterators can be used with <c>foreach</c> and LINQ.
	/// </summary>
	/// <remarks>
	/// The enumerator yields <see cref="Next"/> indefinitely, matching the infinite nature of these iterators.
	/// Callers must bound their iteration (e.g. <c>iterator.Take(n)</c>, or <c>break</c> inside <c>foreach</c>)
	/// to avoid hanging.
	/// </remarks>
	public abstract class CollectionIteratorBase<T> : IEnumerable<T>
	{
		protected IList<T> list;

		protected CollectionIteratorBase(IList<T> list)
		{
			this.list = list;
		}

		public abstract T Next();

		public IEnumerator<T> GetEnumerator()
		{
			while (true)
			{
				yield return Next();
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
