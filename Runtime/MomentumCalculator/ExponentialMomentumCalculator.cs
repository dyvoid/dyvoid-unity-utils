using System;
using UnityEngine;

namespace dyvoid.Utility
{
	/// <summary>
	/// Momentum calculator that decays exponentially over time (a leaky
	/// integrator). Each sample adds its value to the momentum, which then
	/// halves every <c>halfLife</c> seconds. Smoother and cheaper than
	/// <see cref="SlidingWindowMomentumCalculator"/>, but old input fades out
	/// gradually instead of being cut off sharply.
	/// </summary>
	public class ExponentialMomentumCalculator : MomentumCalculator
	{
		private readonly float halfLife;

		private float momentum = 0f;
		private float lastSampleTime = 0f;

		public ExponentialMomentumCalculator(float momentumThreshold, float halfLife = 1f, Func<float> timeProvider = null)
			: base(momentumThreshold, timeProvider)
		{
			this.halfLife = halfLife;
		}

		public override void Clear()
		{
			momentum = 0f;
		}

		protected override void AddSample(float value, float time)
		{
			momentum = GetMomentum(time) + value;
			lastSampleTime = time;
		}

		protected override float GetMomentum(float time)
		{
			if (momentum == 0f) return 0f;
			return momentum * Mathf.Pow(0.5f, (time - lastSampleTime) / halfLife);
		}
	}
}
