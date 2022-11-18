using System.Collections.Generic;
using UnityEngine;

namespace Temple.Utils
{
	public class MomentumCalculator
	{
		public float Momentum => _momentum;
		public bool ExceedsThreshold => (_momentum >= _momentumThreshold);

		private readonly float _sampleTimeRange;
		private readonly float _momentumThreshold;

		private float _momentum = 0f;
		private float _total = 0f;

		private readonly Queue<MomentumSample> _samples = new();


		public MomentumCalculator(float momentumThreshold, float sampleTimeRange = 1f)
		{
			_momentumThreshold = momentumThreshold;
			_sampleTimeRange = sampleTimeRange;
		}

		public bool AddSample(float value)
		{
			_total += value;

			// Add a new sample
			MomentumSample sample = new MomentumSample
			{
				Value = value,
				TimeStamp = Time.time,
			};
			_samples.Enqueue(sample);

			// Remove samples that are older than our timespan
			while (_samples.Count > 1 && (Time.time - _samples.Peek().TimeStamp) > _sampleTimeRange)
			{
				_total -= _samples.Dequeue().Value;
			}

			if (_samples.Count == 1)
			{
				// When we have only one sample it's not enough to calculate a momentum. Set back to 0.
				_momentum = 0f;
			}
			else
			{
				// Only update the momentum when the sample queue reaches at least half of our time span
				float samplesTimeSpan = Time.time - _samples.Peek().TimeStamp;
				if (samplesTimeSpan > _sampleTimeRange * 0.5f)
				{
					_momentum = _total * (_sampleTimeRange / samplesTimeSpan);
				}
			}

			return ExceedsThreshold;
		}
		
		public void Clear()
		{
			_momentum = 0f;
			_total = 0f;
			_samples.Clear();
		}

		private struct MomentumSample
		{
			public float Value;
			public float TimeStamp;
		}
	}
}
