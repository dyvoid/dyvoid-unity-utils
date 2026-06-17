using System.Collections.Generic;
using UnityEngine;

namespace dyvoid.Utility
{
	public class MomentumCalculator
	{
		public float Momentum => momentum;
		public bool ExceedsThreshold => (momentum >= momentumThreshold);

		private readonly float sampleTimeRange;
		private readonly float momentumThreshold;

		private float momentum = 0f;
		private float total = 0f;

		private readonly Queue<MomentumSample> samples = new();


		public MomentumCalculator(float momentumThreshold, float sampleTimeRange = 1f)
		{
			this.momentumThreshold = momentumThreshold;
			this.sampleTimeRange = sampleTimeRange;
		}

		public bool AddSample(float value)
		{
			total += value;

			// Add a new sample
			MomentumSample sample = new MomentumSample
			{
				Value = value,
				TimeStamp = Time.time,
			};
			samples.Enqueue(sample);

			// Remove samples that are older than our timespan
			while (samples.Count > 1 && (Time.time - samples.Peek().TimeStamp) > sampleTimeRange)
			{
				total -= samples.Dequeue().Value;
			}

			if (samples.Count == 1)
			{
				// When we have only one sample it's not enough to calculate a momentum. Set back to 0.
				momentum = 0f;
			}
			else
			{
				// Only update the momentum when the sample queue reaches at least half of our time span
				float samplesTimeSpan = Time.time - samples.Peek().TimeStamp;
				if (samplesTimeSpan > sampleTimeRange * 0.5f)
				{
					momentum = total * (sampleTimeRange / samplesTimeSpan);
				}
			}

			return ExceedsThreshold;
		}
		
		public void Clear()
		{
			momentum = 0f;
			total = 0f;
			samples.Clear();
		}

		private struct MomentumSample
		{
			public float Value;
			public float TimeStamp;
		}
	}
}
