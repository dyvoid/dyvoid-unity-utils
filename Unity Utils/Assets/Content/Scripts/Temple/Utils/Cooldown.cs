using UnityEngine;

namespace Temple.Utils
{
    public class Cooldown
    {
        public float DefaultTime;
        public float Time { get; private set; }

        public bool IsEnabled = true;

        public Cooldown(float defaultTime = 1.0f)
        {
            this.DefaultTime = defaultTime;
            this.Time = defaultTime;
        }

        public void Reset()
        {
            Reset(DefaultTime);
        }

        public void Reset(float time)
        {
            this.Time = time;
        }

        /**
         * Reset to a random value between min and mx
         */
        public void Reset(float min, float max)
        {
            this.Time = Random.Range(min, max);
        }

        /**
         * Returns true when the cooldown has expired
         */
        public bool Update(float deltaTime)
        {
            return IsEnabled && ((Time -= deltaTime) <= 0);
        }

        public bool hasExpired
        {
            get { return (IsEnabled && Time <= 0); }
        }
    }
}