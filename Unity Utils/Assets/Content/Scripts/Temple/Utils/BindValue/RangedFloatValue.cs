using System;
using UnityEngine;

namespace Temple.Utils
{
    [Serializable]
    public class RangedFloatValue : FloatValue
    {
        [SerializeField] private Range _range = new Range();

        public Range Range => _range;

        private RangedFloatValue _leader;


        public RangedFloatValue(float value = 0.0f, float boundary1 = 0.0f, float boundary2 = 1.0f) : base(value)
        {
            this.SetRange(boundary1, boundary2);
        }

        public void SetRange(float boundary1, float boundary2)
        {
            _range.Set(boundary1, boundary2);
            // Yes really. Range/modifiers may cause value to change, so we trigger
            // the range/modifiers by using the value setter.
            value = value;
        }

        public override float value
        {
            set
            {
                base.value = Range.Clamp(value);
            }
        }

        private void OnLeaderValueChanged(float newValue, float oldValue)
        {
            value = _leader.Range.Translate(newValue, Range);
        }

        public void ClearBounds()
        {
            _range.Set(float.MinValue, float.MaxValue);
        }

        public void Follow(RangedFloatValue leader, bool getInitialValue = true)
        {
            if (_leader != null)
            {
                _leader.Unsubscribe(this.OnLeaderValueChanged);
            }

            _leader = leader;
            _leader.Subscribe(this.OnLeaderValueChanged, getInitialValue);
        }

        public void Unfollow()
        {
            if (_leader != null)
            {
                _leader.Unsubscribe(this.OnLeaderValueChanged);
                _leader = null;
            }
        }

        #region ISerializationCallbackReceiver

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            _value = _range.Clamp(_value);
        }

        #endregion


    }
}