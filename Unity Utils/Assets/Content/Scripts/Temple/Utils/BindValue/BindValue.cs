using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Temple.Utils
{
    public class BindValue<T> : IDisposable, ISerializationCallbackReceiver
    {
        // This field is required for inheriting classes to be serializable
        [SerializeField] protected T _value;

        public Action<T, T> ChangedAction;

        public BindValue([Optional] T value)
        {
            this._value = value;
        }

        public virtual T value
        {
            get { return _value; }
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value)) return;

                T oldValue = _value;
                _value = value;

                this.OnValueChanged(_value, oldValue);
            }
        }

        public BindValue<T2> Map<T2>(Func<T, T, T2> handler)
        {
            BindValue<T2> mappedValue = new BindValue<T2>(handler.Invoke(_value, _value));
            Subscribe((to, from) => mappedValue.value = handler.Invoke(to, from), true);
            return mappedValue;
        }

        public static BindValue<T3> Merge<T1, T2, T3>(BindValue<T1> value1, BindValue<T2> value2, Func<T1, T2, T3> combinator)
        {
            BindValue<T3> c = new BindValue<T3>(combinator.Invoke(value1.value, value2.value));
            value1.Subscribe((to, from) => c.value = combinator.Invoke(to, value2.value), true);
            value2.Subscribe((to, from) => c.value = combinator.Invoke(value1.value, to), true);
            return c;
        }

        public BindValue<T3> MergeWith<T2, T3>(BindValue<T2> value, Func<T, T2, T3> combinator)
        {
            return Merge(this, value, combinator);
        }

        public BindValue<T> Subscribe(Action<T, T> callback, bool getInitialValue = false)
        {
            ChangedAction += callback;

            if (getInitialValue)
            {
                callback?.Invoke(_value, _value);
            }

            return this;
        }

        public BindValue<T> Unsubscribe(Action<T, T> callback)
        {
            if (ChangedAction != null)
            {
                ChangedAction -= callback;
            }

            return this;
        }

        protected void OnValueChanged(T newValue, T oldValue)
        {
            ChangedAction?.Invoke(newValue, oldValue);
        }

        public void Dispose()
        {
            ChangedAction = null;
        }

        #region ISerializationCallbackReceiver

        public virtual void OnBeforeSerialize()
        {

        }

        public virtual void OnAfterDeserialize()
        {

        }

        #endregion
    }
}