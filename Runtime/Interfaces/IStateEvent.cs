﻿using System;
using System.Collections.Generic;

namespace SwiftFramework.Core
{
    public class StatefulEvent<T> : IStatefulEvent<T>
    {
        public StatefulEvent(T defaultValue = default)
        {
            Value = defaultValue;
        }

        public T Value { get; private set; } = default;

        public event ValueHanlder<T> OnValueChanged = v => { };

        public event ValueDeltaHadnler<T> OnValueChangedDelta = (oldValue, newValue) => { };

        public override bool Equals(object obj)
        {
            var @event = obj as StatefulEvent<T>;
            return @event != null &&
                   EqualityComparer<T>.Default.Equals(Value, @event.Value);
        }

        public override int GetHashCode()
        {
            return -1937169414 + EqualityComparer<T>.Default.GetHashCode(Value);
        }

        public void SetValueWithoutNotify(T newValue)
        {
            T oldValue = Value;
            Value = newValue;
        }

        public void SetValue(T newValue)
        {
            T oldValue = Value;
            Value = newValue;
            OnValueChanged(newValue);
            OnValueChangedDelta(oldValue, newValue);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public interface IStatefulEvent<T>
    {
        event ValueHanlder<T> OnValueChanged;
        event ValueDeltaHadnler<T> OnValueChangedDelta;
        T Value { get; }
    }

    public delegate void ValueDeltaHadnler<T>(T oldValue, T newValue);
    public delegate void ValueHanlder<T>(T value);
}
