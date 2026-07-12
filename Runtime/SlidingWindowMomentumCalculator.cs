using System;
using System.Collections.Generic;
using UnityEngine;

namespace dyvoid.Utility
{
	/// <summary>
	/// Momentum calculator with a hard cutoff: only samples within the last
	/// <c>sampleTimeRange</c> seconds contribute, older ones are discarded
	/// entirely. Use this when "exactly the last N seconds" matters. For a
	/// smoother and cheaper estimate see
	/// <see cref="ExponentialMomentumCalculator"/>.
	/// </summary>
	public class SlidingWindowMomentumCalculator : MomentumCalculator
	{
		private readonly float sampleTimeRange;

		private float total = 0f;
		private readonly Queue<MomentumSample> samples = new();

		public SlidingWindowMomentumCalculator(float momentumThreshold, float sampleTimeRange = 1f, Func<float> timeProvider = null)
			: base(momentumThreshold, timeProvider)
		{
			this.sampleTimeRange = sampleTimeRange;
		}

		public override void Clear()
		{
			total = 0f;
			samples.Clear();
		}

		protected override void AddSample(float value, float time)
		{
			total += value;
			samples.Enqueue(new MomentumSample
			{
				Value = value,
				TimeStamp = time,
			});
		}

		protected override float GetMomentum(float time)
		{
			// Remove samples that are older than our time span
			while (samples.Count > 0 && (time - samples.Peek().TimeStamp) > sampleTimeRange)
			{
				total -= samples.Dequeue().Value;
			}

			if (samples.Count == 0)
			{
				// Reset the running total so float rounding errors can't
				// accumulate across many enqueue/dequeue cycles
				total = 0f;
				return 0f;
			}

			// A single sample is not enough to estimate a rate
			if (samples.Count == 1) return 0f;

			// Extrapolate a partially filled window to a full one, so the
			// momentum responds during the first samples already. The span is
			// clamped to half the window to cap that extrapolation at 2x.
			float samplesTimeSpan = time - samples.Peek().TimeStamp;
			return total * (sampleTimeRange / Mathf.Max(samplesTimeSpan, sampleTimeRange * 0.5f));
		}

		private struct MomentumSample
		{
			public float Value;
			public float TimeStamp;
		}
	}
}
