using NUnit.Framework;
using dyvoid.Utility;

namespace dyvoid.Utility.Tests
{
	[TestFixture]
	public class ValueRangeTests
	{
		[Test]
		public void Constructor_SetsFromAndTo()
		{
			var r = new ValueRange(2f, 8f);
			Assert.AreEqual(2f, r.From);
			Assert.AreEqual(8f, r.To);
		}

		[Test]
		public void Constructor_Defaults_ZeroToOne()
		{
			var r = new ValueRange();
			Assert.AreEqual(0f, r.From);
			Assert.AreEqual(1f, r.To);
		}

		[Test]
		public void Size_IsPositive_ForAscendingRange()
		{
			var r = new ValueRange(1f, 5f);
			Assert.AreEqual(4f, r.Size, 0.0001f);
		}

		[Test]
		public void Size_IsPositive_ForDescendingRange()
		{
			var r = new ValueRange(5f, 1f);
			Assert.AreEqual(4f, r.Size, 0.0001f);
		}

		[Test]
		public void Direction_Ascending_WhenToGreaterThanFrom()
		{
			var r = new ValueRange(0f, 1f);
			Assert.AreEqual(RangeDirection.Ascending, r.Direction);
		}

		[Test]
		public void Direction_Descending_WhenFromGreaterThanTo()
		{
			var r = new ValueRange(5f, 2f);
			Assert.AreEqual(RangeDirection.Descending, r.Direction);
		}

		[Test]
		public void LowerBoundary_Get_ReturnsSmaller()
		{
			Assert.AreEqual(1f, new ValueRange(1f, 5f).LowerBoundary);
			Assert.AreEqual(1f, new ValueRange(5f, 1f).LowerBoundary);
		}

		[Test]
		public void UpperBoundary_Get_ReturnsLarger()
		{
			Assert.AreEqual(5f, new ValueRange(1f, 5f).UpperBoundary);
			Assert.AreEqual(5f, new ValueRange(5f, 1f).UpperBoundary);
		}

		[Test]
		public void LowerBoundary_Set_ModifiesCorrectField_Ascending()
		{
			var r = new ValueRange(1f, 5f);
			r.LowerBoundary = 0f;
			Assert.AreEqual(0f, r.From);
		}

		[Test]
		public void LowerBoundary_Set_ModifiesCorrectField_Descending()
		{
			var r = new ValueRange(5f, 1f);
			r.LowerBoundary = 0f;
			Assert.AreEqual(0f, r.To);
		}

		[Test]
		public void Contains_True_WhenValueInsideRange()
		{
			var r = new ValueRange(0f, 10f);
			Assert.IsTrue(r.Contains(5f));
		}

		[Test]
		public void Contains_True_AtBoundaries()
		{
			var r = new ValueRange(0f, 10f);
			Assert.IsTrue(r.Contains(0f));
			Assert.IsTrue(r.Contains(10f));
		}

		[Test]
		public void Contains_False_WhenOutsideRange()
		{
			var r = new ValueRange(0f, 10f);
			Assert.IsFalse(r.Contains(-1f));
			Assert.IsFalse(r.Contains(11f));
		}

		[Test]
		public void GetValueAt_Zero_ReturnsFrom()
		{
			var r = new ValueRange(2f, 8f);
			Assert.AreEqual(2f, r.GetValueAt(0f), 0.0001f);
		}

		[Test]
		public void GetValueAt_One_ReturnsTo()
		{
			var r = new ValueRange(2f, 8f);
			Assert.AreEqual(8f, r.GetValueAt(1f), 0.0001f);
		}

		[Test]
		public void GetValueAt_Half_ReturnsMidpoint()
		{
			var r = new ValueRange(0f, 10f);
			Assert.AreEqual(5f, r.GetValueAt(0.5f), 0.0001f);
		}

		[Test]
		public void GetPercentageOf_AtFrom_ReturnsZero()
		{
			var r = new ValueRange(2f, 8f);
			Assert.AreEqual(0f, r.GetPercentageOf(2f), 0.0001f);
		}

		[Test]
		public void GetPercentageOf_AtTo_ReturnsOne()
		{
			var r = new ValueRange(2f, 8f);
			Assert.AreEqual(1f, r.GetPercentageOf(8f), 0.0001f);
		}

		[Test]
		public void Clamp_ValueBelowRange_ReturnsLowerBoundary()
		{
			var r = new ValueRange(2f, 8f);
			Assert.AreEqual(2f, r.Clamp(-5f), 0.0001f);
		}

		[Test]
		public void Clamp_ValueAboveRange_ReturnsUpperBoundary()
		{
			var r = new ValueRange(2f, 8f);
			Assert.AreEqual(8f, r.Clamp(100f), 0.0001f);
		}

		[Test]
		public void Clamp_ValueInRange_ReturnsValue()
		{
			var r = new ValueRange(2f, 8f);
			Assert.AreEqual(5f, r.Clamp(5f), 0.0001f);
		}

		[Test]
		public void Remap_MapsValueToOutputRange()
		{
			var input = new ValueRange(0f, 10f);
			var output = new ValueRange(0f, 1f);
			Assert.AreEqual(0.5f, input.Remap(5f, output), 0.0001f);
		}

		[Test]
		public void Offset_ShiftsBothBoundaries()
		{
			var r = new ValueRange(1f, 5f);
			var shifted = r.Offset(2f);
			Assert.AreEqual(3f, shifted.From, 0.0001f);
			Assert.AreEqual(7f, shifted.To, 0.0001f);
		}

		[Test]
		public void Operator_Plus_OffsetsRange()
		{
			var r = new ValueRange(1f, 5f) + 1f;
			Assert.AreEqual(2f, r.From, 0.0001f);
			Assert.AreEqual(6f, r.To, 0.0001f);
		}

		[Test]
		public void Operator_Minus_OffsetsRange()
		{
			var r = new ValueRange(2f, 6f) - 1f;
			Assert.AreEqual(1f, r.From, 0.0001f);
			Assert.AreEqual(5f, r.To, 0.0001f);
		}

		[Test]
		public void Equals_True_ForSameValues()
		{
			var a = new ValueRange(1f, 5f);
			var b = new ValueRange(1f, 5f);
			Assert.IsTrue(a.Equals(b));
		}

		[Test]
		public void Equals_False_ForDifferentValues()
		{
			var a = new ValueRange(1f, 5f);
			var b = new ValueRange(1f, 6f);
			Assert.IsFalse(a.Equals(b));
		}

		[Test]
		public void GetHashCode_Equal_ForEqualRanges()
		{
			var a = new ValueRange(1f, 5f);
			var b = new ValueRange(1f, 5f);
			Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
		}

		[Test]
		public void Set_UpdatesFromAndTo()
		{
			var r = new ValueRange(0f, 1f);
			r.Set(3f, 7f);
			Assert.AreEqual(3f, r.From);
			Assert.AreEqual(7f, r.To);
		}
	}
}
