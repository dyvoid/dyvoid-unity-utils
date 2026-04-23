using System;

namespace Utility
{
	/// <summary>
	/// An <see cref="ObservableValue{TTo}"/> whose value is derived from a single source observable
	/// via a transform function. Disposing it unsubscribes from the source.
	/// </summary>
	internal sealed class MappedObservableValue<TFrom, TTo> : ObservableValue<TTo>
	{
		private ObservableValue<TFrom> source;
		private Action<TFrom, TFrom> subscription;

		public MappedObservableValue(ObservableValue<TFrom> source, Func<TFrom, TFrom, TTo> handler): base(handler.Invoke(source.Value, source.Value))
		{
			this.source = source;
			this.subscription = (to, from) => Value = handler.Invoke(to, from);
			this.source.Subscribe(this.subscription);
		}

		public override void Dispose()
		{
			if (source != null)
			{
				source.Unsubscribe(subscription);
				source = null;
				subscription = null;
			}

			base.Dispose();
		}
	}
}
