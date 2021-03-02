/* using DG.Tweening;

namespace Temple.Utils
{
    public static class FloatValueExtensions
    {
        public static Tween animate(this FloatValue floatValue, float from, float to, float duration, Ease ease = Ease.Linear, float delay = 0.0f)
        {
            floatValue.value = from;
            return DOTween.To(() => floatValue.value, (newValue) => floatValue.value = newValue, to, duration).SetEase(ease).SetDelay(delay);
        }

        public static Tween animateFrom(this FloatValue floatValue, float from, float duration, Ease ease = Ease.Linear, float delay = 0.0f)
        {
            return floatValue.animate(from, floatValue.value, duration, ease, delay);
        }

        public static Tween animateTo(this FloatValue floatValue, float to, float duration, Ease ease = Ease.Linear, float delay = 0.0f)
        {
            return floatValue.animate(floatValue.value, to, duration, ease, delay);
        }
    }
} */