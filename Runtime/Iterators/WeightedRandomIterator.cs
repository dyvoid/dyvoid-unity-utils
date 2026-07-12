using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace dyvoid.Utility
{
	/// <summary>
	/// Infinitely returns random items where each item's selection probability is
	/// proportional to its weight. Items with a weight of 0 are never returned.
	/// Useful for loot tables and rare/common variation in VFX or audio.
	/// </summary>
	/// <remarks>
	/// Unlike <see cref="IterationMode.Random"/> (a shuffle bag that returns every item
	/// once per cycle), this iterator draws independently on every call, so repeats can
	/// occur back to back.
	/// </remarks>
	public class WeightedRandomIterator<T> : CollectionIteratorBase<T>
	{
		private readonly IList<float> weights;

		/// <summary>
		/// Creates a weighted random iterator from parallel item and weight lists.
		/// The lists are referenced, not copied, so external changes are picked up;
		/// both lists must remain the same length.
		/// </summary>
		/// <param name="list">The items to draw from</param>
		/// <param name="weights">The weight of each item, parallel to <paramref name="list"/>.
		/// Weights must be non-negative and at least one must be positive.</param>
		public WeightedRandomIterator(IList<T> list, IList<float> weights) : base(list)
		{
			if (list == null) throw new ArgumentNullException(nameof(list));
			this.weights = weights ?? throw new ArgumentNullException(nameof(weights));

			ValidateWeights();
		}

		/// <summary>
		/// Creates a weighted random iterator from item/weight pairs.
		/// The pairs are copied into internal lists, so later changes to
		/// <paramref name="weightedItems"/> are not picked up.
		/// </summary>
		/// <param name="weightedItems">The items to draw from, each paired with its weight.
		/// Weights must be non-negative and at least one must be positive.</param>
		public WeightedRandomIterator(IList<(T item, float weight)> weightedItems)
			: base(new List<T>())
		{
			if (weightedItems == null) throw new ArgumentNullException(nameof(weightedItems));

			var weightsList = new List<float>(weightedItems.Count);
			foreach (var (item, weight) in weightedItems)
			{
				list.Add(item);
				weightsList.Add(weight);
			}
			weights = weightsList;

			ValidateWeights();
		}

		/// <summary>
		/// Returns a random item, selected proportionally to the item weights.
		/// </summary>
		/// <returns>The selected item</returns>
		/// <exception cref="InvalidOperationException">Thrown when the collection is empty,
		/// the item and weight counts have diverged, or no weight is positive.</exception>
		public override T Next()
		{
			if (list.Count == 0)
			{
				throw new InvalidOperationException("Cannot iterate over an empty collection");
			}

			if (weights.Count != list.Count)
			{
				throw new InvalidOperationException(
					$"Item count ({list.Count}) and weight count ({weights.Count}) have diverged");
			}

			float total = 0f;
			for (int i = 0; i < weights.Count; i++)
			{
				if (weights[i] < 0f)
				{
					throw new InvalidOperationException($"Weight at index {i} is negative ({weights[i]})");
				}
				total += weights[i];
			}

			if (total <= 0f)
			{
				throw new InvalidOperationException("At least one weight must be positive");
			}

			float roll = Random.value * total;
			float cumulative = 0f;
			int lastPositiveIndex = -1;

			for (int i = 0; i < weights.Count; i++)
			{
				if (weights[i] <= 0f) continue;

				cumulative += weights[i];
				lastPositiveIndex = i;

				if (roll < cumulative)
				{
					return list[i];
				}
			}

			// Float rounding can leave roll == total; fall back to the last selectable item.
			return list[lastPositiveIndex];
		}

		private void ValidateWeights()
		{
			if (weights.Count != list.Count)
			{
				throw new ArgumentException(
					$"Item count ({list.Count}) and weight count ({weights.Count}) must match");
			}

			bool anyPositive = false;
			for (int i = 0; i < weights.Count; i++)
			{
				if (weights[i] < 0f)
				{
					throw new ArgumentException($"Weight at index {i} is negative ({weights[i]})");
				}
				if (weights[i] > 0f) anyPositive = true;
			}

			if (!anyPositive)
			{
				throw new ArgumentException("At least one weight must be positive");
			}
		}
	}
}
