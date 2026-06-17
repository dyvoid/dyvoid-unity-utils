using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using dyvoid.Utility;

namespace dyvoid.Utility.Tests
{
	[TestFixture]
	public class IteratorTests
	{
		// ── InfiniteIterator / Sequential ──────────────────────────────────────

		[Test]
		public void Sequential_IteratesInOrder()
		{
			var list = new List<int> { 10, 20, 30 };
			var iter = new InfiniteIterator<int>(list, IterationMode.Sequential);
			Assert.AreEqual(10, iter.Next());
			Assert.AreEqual(20, iter.Next());
			Assert.AreEqual(30, iter.Next());
		}

		[Test]
		public void Sequential_WrapsAround()
		{
			var list = new List<int> { 1, 2 };
			var iter = new InfiniteIterator<int>(list, IterationMode.Sequential);
			iter.Next(); iter.Next();
			Assert.AreEqual(1, iter.Next());
		}

		[Test]
		public void Sequential_SingleElement_AlwaysReturnsThatElement()
		{
			var list = new List<int> { 99 };
			var iter = new InfiniteIterator<int>(list, IterationMode.Sequential);
			for (int i = 0; i < 5; i++)
			{
				Assert.AreEqual(99, iter.Next());
			}
		}

		// ── PingPong ───────────────────────────────────────────────────────────

		[Test]
		public void PingPong_TraversesForwardThenBackward()
		{
			var list = new List<int> { 0, 1, 2, 3 };
			var iter = new InfiniteIterator<int>(list, IterationMode.PingPong);
			var results = new List<int>();
			for (int i = 0; i < 7; i++) results.Add(iter.Next());
			CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 2, 1, 0 }, results);
		}

		[Test]
		public void PingPong_SingleElement_AlwaysReturnsZero()
		{
			var list = new List<string> { "a" };
			var iter = new InfiniteIterator<string>(list, IterationMode.PingPong);
			for (int i = 0; i < 5; i++)
			{
				Assert.AreEqual("a", iter.Next());
			}
		}

		// ── Random ────────────────────────────────────────────────────────────

		[Test]
		public void Random_AllIndicesReturnedOncePerCycle()
		{
			var list = new List<int> { 0, 1, 2, 3, 4 };
			var iter = new InfiniteIterator<int>(list, IterationMode.Random);
			var seen = new HashSet<int>();
			for (int i = 0; i < list.Count; i++)
			{
				seen.Add(iter.Next());
			}
			Assert.AreEqual(list.Count, seen.Count);
		}

		[Test]
		public void Random_SingleElement_AlwaysReturnsThatElement()
		{
			var list = new List<int> { 7 };
			var iter = new InfiniteIterator<int>(list, IterationMode.Random);
			for (int i = 0; i < 5; i++)
			{
				Assert.AreEqual(7, iter.Next());
			}
		}

		// ── Empty collection ──────────────────────────────────────────────────

		[Test]
		public void InfiniteIterator_EmptyList_Throws()
		{
			var iter = new InfiniteIterator<int>(new List<int>());
			Assert.Throws<InvalidOperationException>(() => iter.Next());
		}

		// ── IEnumerable / Take ────────────────────────────────────────────────

		[Test]
		public void InfiniteIterator_Take_ReturnsCorrectCount()
		{
			var list = new List<int> { 1, 2, 3 };
			var iter = new InfiniteIterator<int>(list);
			var taken = iter.Take(7).ToList();
			Assert.AreEqual(7, taken.Count);
		}

		// ── CombinedIterator ──────────────────────────────────────────────────

		[Test]
		public void CombinedIterator_Next_ReturnsFromFirstCollection()
		{
			var a = new List<string> { "a1", "a2" };
			var b = new List<string> { "b1", "b2" };
			var combined = new CombinedIterator<string>(a, b);
			Assert.AreEqual("a1", combined.Next());
			Assert.AreEqual("a2", combined.Next());
		}

		[Test]
		public void CombinedIterator_NextCollection_SwitchesToSecond()
		{
			var a = new List<string> { "a1" };
			var b = new List<string> { "b1" };
			var combined = new CombinedIterator<string>(a, b);
			combined.NextCollection();
			Assert.AreEqual("b1", combined.Next());
		}

		[Test]
		public void CombinedIterator_NextCollection_WrapsAround()
		{
			var a = new List<int> { 1 };
			var b = new List<int> { 2 };
			var combined = new CombinedIterator<int>(a, b);
			combined.NextCollection();
			combined.NextCollection();
			Assert.AreEqual(1, combined.Next());
		}

		[Test]
		public void CombinedIterator_SetCollection_SwitchesToIndex()
		{
			var a = new List<int> { 10 };
			var b = new List<int> { 20 };
			var combined = new CombinedIterator<int>(a, b);
			combined.SetCollection(1);
			Assert.AreEqual(20, combined.Next());
		}

		[Test]
		public void CombinedIterator_SetCollection_OutOfRange_Throws()
		{
			var combined = new CombinedIterator<int>(new List<int> { 1 });
			Assert.Throws<ArgumentOutOfRangeException>(() => combined.SetCollection(5));
		}

		[Test]
		public void CombinedIterator_EmptyCollections_Throws()
		{
			Assert.Throws<ArgumentException>(() =>
				new CombinedIterator<int>(new List<int>(), new List<int>()));
		}

		[Test]
		public void CombinedIterator_NoCollections_Throws()
		{
			Assert.Throws<ArgumentException>(() =>
				new CombinedIterator<int>());
		}

		// ── GroupedIterator ───────────────────────────────────────────────────

		[Test]
		public void GroupedIterator_NextGroup_ReturnsCorrectSize()
		{
			var list = new List<int> { 1, 2, 3, 4, 5, 6 };
			var grouped = new GroupedIterator<int>(list, 3);
			var group = grouped.NextGroup();
			Assert.AreEqual(3, group.Count);
		}

		[Test]
		public void GroupedIterator_NextGroup_ReturnsSequentialItems()
		{
			var list = new List<int> { 10, 20, 30, 40 };
			var grouped = new GroupedIterator<int>(list, 2);
			var group = grouped.NextGroup();
			CollectionAssert.AreEqual(new[] { 10, 20 }, group);
		}

		[Test]
		public void GroupedIterator_NextGroup_WithCount_ReturnsSpecifiedSize()
		{
			var list = new List<int> { 1, 2, 3, 4 };
			var grouped = new GroupedIterator<int>(list, 2);
			var group = grouped.NextGroup(4);
			Assert.AreEqual(4, group.Count);
		}

		[Test]
		public void GroupedIterator_ZeroSize_Throws()
		{
			Assert.Throws<ArgumentException>(() =>
				new GroupedIterator<int>(new List<int> { 1 }, 0));
		}

		[Test]
		public void GroupedIterator_NextGroup_ZeroCount_Throws()
		{
			var grouped = new GroupedIterator<int>(new List<int> { 1, 2 }, 1);
			Assert.Throws<ArgumentException>(() => grouped.NextGroup(0));
		}

		// ── TimedIterator ─────────────────────────────────────────────────────

		[Test]
		public void TimedIterator_AtTimeZero_ReturnsFirstItem()
		{
			var list = new List<string> { "a", "b", "c" };
			var iter = new TimedIterator<string>(list, 1.0f);
			Assert.AreEqual("a", iter.Next());
		}

		[Test]
		public void TimedIterator_AfterOneInterval_ReturnsSecondItem()
		{
			var list = new List<string> { "a", "b", "c" };
			var iter = new TimedIterator<string>(list, 1.0f);
			iter.UpdateTime(1.0f);
			Assert.AreEqual("b", iter.Next());
		}

		[Test]
		public void TimedIterator_WrapsAroundAfterLastItem()
		{
			var list = new List<string> { "a", "b" };
			var iter = new TimedIterator<string>(list, 1.0f);
			iter.UpdateTime(2.0f);
			Assert.AreEqual("a", iter.Next());
		}

		[Test]
		public void TimedIterator_SetInterval_ChangesAdvanceRate()
		{
			var list = new List<string> { "a", "b", "c" };
			var iter = new TimedIterator<string>(list, 1.0f);
			iter.SetInterval(2.0f);
			iter.UpdateTime(1.5f);
			Assert.AreEqual("a", iter.Next());
			iter.UpdateTime(1.0f);
			Assert.AreEqual("b", iter.Next());
		}

		[Test]
		public void TimedIterator_InvalidInterval_Throws()
		{
			Assert.Throws<ArgumentException>(() =>
				new TimedIterator<int>(new List<int> { 1 }, 0f));
		}

		[Test]
		public void TimedIterator_SetInvalidInterval_Throws()
		{
			var iter = new TimedIterator<int>(new List<int> { 1 }, 1.0f);
			Assert.Throws<ArgumentException>(() => iter.SetInterval(-1f));
		}

		[Test]
		public void TimedIterator_EmptyList_Throws()
		{
			var iter = new TimedIterator<int>(new List<int>(), 1.0f);
			Assert.Throws<InvalidOperationException>(() => iter.Next());
		}
	}
}
