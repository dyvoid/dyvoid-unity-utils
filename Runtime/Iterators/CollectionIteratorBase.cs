using System.Collections;
using System.Collections.Generic;

namespace dyvoid.Utility
{
	/// <summary>
	/// Base class for collection iterators.
	/// Implements <see cref="IEnumerable{T}"/> so iterators can be used with <c>foreach</c> and LINQ.
	/// </summary>
	/// <remarks>
	/// The enumerator yields <see cref="Next"/> indefinitely, matching the infinite nature of these iterators.
	/// Callers must bound their iteration (e.g. <c>iterator.Take(n)</c>, or <c>break</c> inside <c>foreach</c>)
	/// to avoid hanging. Finite subclasses (e.g. <see cref="LoopingIterator{T}"/>) override
	/// <see cref="GetEnumerator"/> with a terminating enumerator.
	/// </remarks>
	public abstract class CollectionIteratorBase<T> : IEnumerable<T>
	{
		protected IList<T> list;

		protected CollectionIteratorBase(IList<T> list)
		{
			this.list = list;
		}

		public abstract T Next();

		public virtual IEnumerator<T> GetEnumerator()
		{
			while (true)
			{
				yield return Next();
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
