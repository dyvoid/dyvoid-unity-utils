/*using DG.Tweening;

namespace Utility
{
	public static class ObservableFloatExtensions
	{
		public static Tween Animate(this ObservableFloat observableFloat, float from, float to, float duration, Ease ease = Ease.Linear, float delay = 0.0f)
		{
			observableFloat.Value = from;
			return DOTween.To(() => observableFloat.Value, (newValue) => observableFloat.Value = newValue, to, duration).SetEase(ease).SetDelay(delay);
		}

		public static Tween AnimateFrom(this ObservableFloat observableFloat, float from, float duration, Ease ease = Ease.Linear, float delay = 0.0f)
		{
			return observableFloat.Animate(from, observableFloat.Value, duration, ease, delay);
		}

		public static Tween AnimateTo(this ObservableFloat observableFloat, float to, float duration, Ease ease = Ease.Linear, float delay = 0.0f)
		{
			return observableFloat.Animate(observableFloat.Value, to, duration, ease, delay);
		}
	}
}*/
