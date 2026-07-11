using UnityEngine;
using Random = UnityEngine.Random;

namespace dyvoid.Utility
{
	/// <summary>
	/// Simple timeout mechanism that can be run from an Update loop
	/// </summary>
	public class Cooldown
	{
		public float DefaultDuration { get; set; }
		public float TimeLeft { get; set; }

		public bool HasExpired => (TimeLeft <= 0);

		/// <summary>
		/// Constructs a new Cooldown instance
		/// </summary>
		/// <param name="defaultDuration">This duration will
		/// be used as the initial time and will be re-used on every
		/// <c>Reset()</c> call.</param>
		public Cooldown(float defaultDuration = 1.0f)
		{
			this.DefaultDuration = defaultDuration;
			this.TimeLeft = defaultDuration;
		}

		/// <summary>
		/// Reset to the default duration
		/// </summary>
		/// <param name="subtractOverflow">If the cooldown has previously
		/// expired, a small overflow may have occurred. This overflow
		/// can be subtracted from the new duration, to keep timings in
		/// sync.</param>
		public void Reset(bool subtractOverflow = false)
		{
			Reset(DefaultDuration, subtractOverflow);
		}

		/// <summary>
		/// Reset to a specific duration
		/// </summary>
		/// <param name="duration">The new duration of the cooldown</param>
		/// <param name="subtractOverflow">If the cooldown has previously
		/// expired, a small overflow will have occurred. This overflow
		/// can be subtracted from the new duration, to keep timings in
		/// sync.</param>
		public void Reset(float duration, bool subtractOverflow = false)
		{
			if (subtractOverflow)
			{
				this.TimeLeft = GetOverflowCorrectedDuration(duration);
			}
			else
			{
				this.TimeLeft = duration;
			}
		}

		/// <summary>
		/// Reset to a random value between min and max duration (both values are inclusive)
		/// </summary>
		/// <param name="minDuration"></param>
		/// <param name="maxDuration"></param>
		public void ResetRandom(float minDuration, float maxDuration)
		{
			this.TimeLeft = Random.Range(minDuration, maxDuration);
		}

		/// <summary>
		/// Update the time of the Cooldown
		/// </summary>
		/// <param name="deltaTime"></param>
		/// <returns>Whether the cooldown has expired</returns>
		public bool UpdateTime(float deltaTime)
		{
			return ((TimeLeft -= deltaTime) <= 0);
		}

		private float GetOverflowCorrectedDuration(float duration)
		{
			// Repeat with a non-positive length yields NaN; nothing to correct against.
			if (TimeLeft >= 0 || duration <= 0f) return duration;

			float correctedDuration = Mathf.Repeat(TimeLeft, duration);

			// Safeguard against cases where a Repeat call may cause the new duration
			// to become 0. In that case we just want to return the duration as-is.
			if (correctedDuration > 0f)
			{
				return correctedDuration;
			}
			else
			{
				return duration;
			}
		}
	}
}
