using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MediaMonks.Utility
{
    public class ObservableValue<T> : IDisposable, ISerializationCallbackReceiver
    {
        // This field is required for inheriting classes to be serializable
        [SerializeField] protected T _value;

        public Action<T, T> ChangedAction;

        public ObservableValue([Optional] T value)
        {
            this._value = value;
        }

        public virtual T Value
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

        public ObservableValue<T2> Map<T2>(Func<T, T, T2> handler)
        {
            ObservableValue<T2> mappedValue = new ObservableValue<T2>(handler.Invoke(_value, _value));
            Subscribe((to, from) => mappedValue.Value = handler.Invoke(to, from), true);
            return mappedValue;
        }

        public static ObservableValue<T3> Merge<T1, T2, T3>(ObservableValue<T1> value1, ObservableValue<T2> value2, Func<T1, T2, T3> combinator)
        {
            ObservableValue<T3> c = new ObservableValue<T3>(combinator.Invoke(value1.Value, value2.Value));
            value1.Subscribe((to, from) => c.Value = combinator.Invoke(to, value2.Value), true);
            value2.Subscribe((to, from) => c.Value = combinator.Invoke(value1.Value, to), true);
            return c;
        }

        public ObservableValue<T3> MergeWith<T2, T3>(ObservableValue<T2> value, Func<T, T2, T3> combinator)
        {
            return Merge(this, value, combinator);
        }

        public ObservableValue<T> Subscribe(Action<T, T> callback, bool getInitialValue = false)
        {
            ChangedAction += callback;

            if (getInitialValue)
            {
                callback?.Invoke(_value, _value);
            }

            return this;
        }

        public ObservableValue<T> Unsubscribe(Action<T, T> callback)
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
