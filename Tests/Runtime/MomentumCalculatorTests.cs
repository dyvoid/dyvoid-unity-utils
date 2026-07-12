using NUnit.Framework;
using dyvoid.Utility;

namespace dyvoid.Utility.Tests
{
	[TestFixture]
	public class SlidingWindowMomentumCalculatorTests
	{
		private float time;
		private SlidingWindowMomentumCalculator calculator;

		[SetUp]
		public void SetUp()
		{
			time = 0f;
			calculator = new SlidingWindowMomentumCalculator(
				momentumThreshold: 5f, sampleTimeRange: 1f, timeProvider: () => time);
		}

		[Test]
		public void Momentum_IsZero_Initially()
		{
			Assert.AreEqual(0f, calculator.Momentum);
		}

		[Test]
		public void Momentum_IsZero_WithSingleSample()
		{
			calculator.AddSample(3f);
			Assert.AreEqual(0f, calculator.Momentum);
		}

		[Test]
		public void Momentum_SumsSamples_OverFullWindow()
		{
			calculator.AddSample(1f);
			time = 1f;
			calculator.AddSample(2f);
			Assert.AreEqual(3f, calculator.Momentum, 0.0001f);
		}

		[Test]
		public void Momentum_ExtrapolatesPartialWindow()
		{
			calculator.AddSample(1f);
			time = 0.75f;
			calculator.AddSample(1f);
			// 2 units over 0.75s extrapolated to a 1s window
			Assert.AreEqual(2f / 0.75f, calculator.Momentum, 0.0001f);
		}

		[Test]
		public void Momentum_CapsExtrapolationAtTwice()
		{
			calculator.AddSample(1f);
			time = 0.1f;
			calculator.AddSample(1f);
			Assert.AreEqual(4f, calculator.Momentum, 0.0001f);
		}

		[Test]
		public void Momentum_DropsExpiredSamples()
		{
			calculator.AddSample(4f);
			time = 0.5f;
			calculator.AddSample(4f);
			time = 1.2f;
			calculator.AddSample(4f);
			// The t=0 sample has expired; 8 units over the remaining 0.7s span
			Assert.AreEqual(8f / 0.7f, calculator.Momentum, 0.0001f);
		}

		[Test]
		public void Momentum_Decays_WithoutNewSamples()
		{
			calculator.AddSample(4f);
			time = 0.5f;
			calculator.AddSample(4f);
			Assert.Greater(calculator.Momentum, 0f);

			time = 2f;
			Assert.AreEqual(0f, calculator.Momentum);
		}

		[Test]
		public void AddSample_ReturnsTrue_WhenThresholdExceeded()
		{
			calculator.AddSample(3f);
			time = 1f;
			Assert.IsTrue(calculator.AddSample(3f));
			Assert.IsTrue(calculator.ExceedsThreshold);
		}

		[Test]
		public void AddSample_ReturnsFalse_BelowThreshold()
		{
			calculator.AddSample(1f);
			time = 1f;
			Assert.IsFalse(calculator.AddSample(1f));
			Assert.IsFalse(calculator.ExceedsThreshold);
		}

		[Test]
		public void Clear_ResetsMomentum()
		{
			calculator.AddSample(4f);
			time = 0.5f;
			calculator.AddSample(4f);
			calculator.Clear();
			Assert.AreEqual(0f, calculator.Momentum);
		}
	}

	[TestFixture]
	public class ExponentialMomentumCalculatorTests
	{
		private float time;
		private ExponentialMomentumCalculator calculator;

		[SetUp]
		public void SetUp()
		{
			time = 0f;
			calculator = new ExponentialMomentumCalculator(
				momentumThreshold: 5f, halfLife: 1f, timeProvider: () => time);
		}

		[Test]
		public void Momentum_IsZero_Initially()
		{
			Assert.AreEqual(0f, calculator.Momentum);
		}

		[Test]
		public void FirstSample_SetsMomentum()
		{
			calculator.AddSample(2f);
			Assert.AreEqual(2f, calculator.Momentum, 0.0001f);
		}

		[Test]
		public void Momentum_Halves_AfterHalfLife()
		{
			calculator.AddSample(2f);
			time = 1f;
			Assert.AreEqual(1f, calculator.Momentum, 0.0001f);
		}

		[Test]
		public void Momentum_Decays_ContinuouslyWithoutNewSamples()
		{
			calculator.AddSample(2f);
			time = 3f;
			Assert.AreEqual(0.25f, calculator.Momentum, 0.0001f);
		}

		[Test]
		public void Samples_AccumulateWithDecay()
		{
			calculator.AddSample(2f);
			time = 1f;
			calculator.AddSample(2f);
			// First sample decayed to 1, plus the new sample
			Assert.AreEqual(3f, calculator.Momentum, 0.0001f);
		}

		[Test]
		public void AddSample_ReturnsTrue_WhenThresholdExceeded()
		{
			Assert.IsTrue(calculator.AddSample(6f));
			Assert.IsTrue(calculator.ExceedsThreshold);
		}

		[Test]
		public void AddSample_ReturnsFalse_BelowThreshold()
		{
			Assert.IsFalse(calculator.AddSample(3f));
			Assert.IsFalse(calculator.ExceedsThreshold);
		}

		[Test]
		public void Clear_ResetsMomentum()
		{
			calculator.AddSample(6f);
			calculator.Clear();
			Assert.AreEqual(0f, calculator.Momentum);
		}
	}
}
