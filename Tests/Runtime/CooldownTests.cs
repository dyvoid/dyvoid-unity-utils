using NUnit.Framework;
using dyvoid.Utility;

namespace dyvoid.Utility.Tests
{
	[TestFixture]
	public class CooldownTests
	{
		[Test]
		public void Constructor_SetsDefaultDurationAndTimeLeft()
		{
			var cd = new Cooldown(2.5f);
			Assert.AreEqual(2.5f, cd.DefaultDuration);
			Assert.AreEqual(2.5f, cd.TimeLeft);
		}

		[Test]
		public void Constructor_DefaultValue_IsOne()
		{
			var cd = new Cooldown();
			Assert.AreEqual(1.0f, cd.DefaultDuration);
		}

		[Test]
		public void HasExpired_IsFalse_WhenTimeRemains()
		{
			var cd = new Cooldown(1.0f);
			Assert.IsFalse(cd.HasExpired);
		}

		[Test]
		public void HasExpired_IsTrue_WhenTimeLeftIsZero()
		{
			var cd = new Cooldown(1.0f);
			cd.Reset(0f);
			Assert.IsTrue(cd.HasExpired);
		}

		[Test]
		public void UpdateTime_DecrementsTimeLeft()
		{
			var cd = new Cooldown(1.0f);
			cd.UpdateTime(0.4f);
			Assert.AreEqual(0.6f, cd.TimeLeft, 0.0001f);
		}

		[Test]
		public void UpdateTime_ReturnsFalse_WhenNotExpired()
		{
			var cd = new Cooldown(1.0f);
			bool expired = cd.UpdateTime(0.5f);
			Assert.IsFalse(expired);
		}

		[Test]
		public void UpdateTime_ReturnsTrue_WhenExpired()
		{
			var cd = new Cooldown(1.0f);
			bool expired = cd.UpdateTime(1.5f);
			Assert.IsTrue(expired);
		}

		[Test]
		public void UpdateTime_AllowsNegativeTimeLeft()
		{
			var cd = new Cooldown(1.0f);
			cd.UpdateTime(2.0f);
			Assert.Less(cd.TimeLeft, 0f);
		}

		[Test]
		public void Reset_RestoresDefaultDuration()
		{
			var cd = new Cooldown(1.0f);
			cd.UpdateTime(0.8f);
			cd.Reset();
			Assert.AreEqual(1.0f, cd.TimeLeft, 0.0001f);
		}

		[Test]
		public void Reset_Float_SetsExplicitDuration()
		{
			var cd = new Cooldown(1.0f);
			cd.Reset(3.0f);
			Assert.AreEqual(3.0f, cd.TimeLeft, 0.0001f);
		}

		[Test]
		public void Reset_WithSubtractOverflow_CorrectsDuration()
		{
			var cd = new Cooldown(1.0f);
			cd.UpdateTime(1.5f);
			cd.Reset(subtractOverflow: true);
			Assert.Less(cd.TimeLeft, 1.0f);
			Assert.Greater(cd.TimeLeft, 0f);
		}

		[Test]
		public void Reset_WithSubtractOverflow_NoOverflow_UsesDurationAsIs()
		{
			var cd = new Cooldown(1.0f);
			cd.Reset(subtractOverflow: true);
			Assert.AreEqual(1.0f, cd.TimeLeft, 0.0001f);
		}

		[Test]
		public void ResetRandom_ProducesValueInRange()
		{
			var cd = new Cooldown(1.0f);
			for (int i = 0; i < 20; i++)
			{
				cd.ResetRandom(0.5f, 2.0f);
				Assert.GreaterOrEqual(cd.TimeLeft, 0.5f);
				Assert.LessOrEqual(cd.TimeLeft, 2.0f);
			}
		}
	}
}
