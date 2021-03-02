using System;
using UnityEngine;

namespace Temple.Utils
{
    [Serializable]
    public class FloatValue : BindValue<float>
    {
        public FloatValue(float value = 0.0f) : base(value) { }
    }
}