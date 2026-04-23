using System;
using System.Collections.Generic;
using UnityEngine;

namespace Iterators
{
	/// <summary>
	/// Returns items based on time intervals. Loops infinitely through the collection.
	/// </summary>
	public class TimedIterator<T> : CollectionIteratorBase<T>
	{
		private float interval;
		private float accumulatedTime = 0f;

		/// <summary>
		/// Creates a timed iterator.
		/// </summary>
		/// <param name="list">The collection to iterate over</param>
		/// <param name="interval">Time interval in seconds between items</param>
		public TimedIterator(IList<T> list, float interval) : base(list)
		{
			if (interval <= 0)
			{
				throw new ArgumentException("Interval must be greater than zero", nameof(interval));
			}

			this.interval = interval;
		}

		/// <summary>
		/// Update the accumulated time.
		/// </summary>
		/// <param name="deltaTime">Time elapsed since last update</param>
		public void UpdateTime(float deltaTime)
		{
			accumulatedTime = Mathf.Max(0f, accumulatedTime + deltaTime);
		}

		/// <summary>
		/// Returns the current item based on accumulated time. Loops infinitely through the collection.
		/// </summary>
		/// <returns>The current item for the current time interval</returns>
		public override T Next()
		{
			if (list.Count == 0)
			{
				throw new InvalidOperationException("Cannot iterate over an empty collection");
			}

			// Calculate which item to return based on accumulated time
			int itemIndex = Mathf.FloorToInt(accumulatedTime / interval) % list.Count;
			
			return list[itemIndex];
		}
		
		/// <summary>
		/// Sets a new time interval.
		/// </summary>
		/// <param name="newInterval">New time interval in seconds</param>
		public void SetInterval(float newInterval)
		{
			if (newInterval <= 0)
			{
				throw new ArgumentException("Interval must be greater than zero", nameof(newInterval));
			}
			
			this.interval = newInterval;
		}
	}
}
