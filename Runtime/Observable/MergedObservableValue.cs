using System;

namespace dyvoid.Utility
{
	/// <summary>
	/// An <see cref="ObservableValue{T3}"/> whose value is derived from two source observables
	/// via a combinator function. Disposing it unsubscribes from both sources.
	/// </summary>
	internal sealed class MergedObservableValue<T1, T2, T3> : ObservableValue<T3>
	{
		private ObservableValue<T1> source1;
		private ObservableValue<T2> source2;
		private Action<T1, T1> subscription1;
		private Action<T2, T2> subscription2;

		public MergedObservableValue(ObservableValue<T1> source1, ObservableValue<T2> source2, Func<T1, T2, T3> combinator): base(combinator.Invoke(source1.Value, source2.Value))
		{
			this.source1 = source1;
			this.source2 = source2;

			this.subscription1 = (to, from) => Value = combinator.Invoke(to, this.source2.Value);
			this.subscription2 = (to, from) => Value = combinator.Invoke(this.source1.Value, to);

			this.source1.Subscribe(this.subscription1);
			this.source2.Subscribe(this.subscription2);
		}

		public override void Dispose()
		{
			if (source1 != null)
			{
				source1.Unsubscribe(subscription1);
				source1 = null;
				subscription1 = null;
			}

			if (source2 != null)
			{
				source2.Unsubscribe(subscription2);
				source2 = null;
				subscription2 = null;
			}

			base.Dispose();
		}
	}
}
