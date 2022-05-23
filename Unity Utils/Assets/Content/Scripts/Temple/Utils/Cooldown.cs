using UnityEngine;

namespace Temple.Utils
{
    public class Cooldown
    {
        public float DefaultDuration;
        public float Time { get; private set; }

        public bool IsEnabled = true;

        public bool HasExpired => (IsEnabled && Time <= 0);


        public Cooldown(float defaultDuration = 1.0f)
        {
            this.DefaultDuration = defaultDuration;
            this.Time = defaultDuration;
        }

        public void Reset()
        {
            Reset(DefaultDuration);
        }

        public void Reset(float duration)
        {
            this.Time = duration;
        }

        /**
         * Reset to a random value between min and max
         */
        public void Reset(float minDuration, float maxDuration)
        {
            this.Time = Random.Range(minDuration, maxDuration);
        }

        /**
         * Returns true when the cooldown has expired
         */
        public bool Update(float deltaTime)
        {
            return IsEnabled && ((Time -= deltaTime) <= 0);
        }
    }
}