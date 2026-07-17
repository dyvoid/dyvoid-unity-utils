using System;
using UnityEngine;

namespace dyvoid.Utility
{
	/// <summary>
	/// Base class for momentum calculators, which track the rate of
	/// accumulated input over recent time. Feed input values via
	/// <c>AddSample()</c> and read <c>Momentum</c> or <c>ExceedsThreshold</c>
	/// at any time; the momentum stays up to date (and decays) even when no
	/// new samples arrive.
	///
	/// Two implementations are available:
	/// <see cref="SlidingWindowMomentumCalculator"/> counts exactly the last
	/// N seconds of input, <see cref="ExponentialMomentumCalculator"/> lets
	/// input fade out gradually.
	/// </summary>
	public abstract class MomentumCalculator
	{
		public float Momentum => GetMomentum(Now);
		public bool ExceedsThreshold => (Momentum >= momentumThreshold);

		private readonly float momentumThreshold;
		private readonly Func<float> timeProvider;

		protected float Now => timeProvider();

		/// <param name="momentumThreshold">Momentum value at which
		/// <c>ExceedsThreshold</c> becomes true</param>
		/// <param name="timeProvider">Optional clock override, mainly useful
		/// for tests. Defaults to <c>Time.time</c>.</param>
		protected MomentumCalculator(float momentumThreshold, Func<float> timeProvider = null)
		{
			this.momentumThreshold = momentumThreshold;
			this.timeProvider = timeProvider ?? (() => Time.time);
		}

		/// <summary>
		/// Add an input sample at the current time
		/// </summary>
		/// <returns>Whether the momentum now exceeds the threshold</returns>
		public bool AddSample(float value)
		{
			AddSample(value, Now);
			return ExceedsThreshold;
		}

		/// <summary>
		/// Reset the momentum back to 0, discarding all samples
		/// </summary>
		public abstract void Clear();

		protected abstract void AddSample(float value, float time);
		protected abstract float GetMomentum(float time);
	}
}
