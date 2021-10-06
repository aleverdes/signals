using System;
using System.Collections.Generic;

namespace AffenSignals
{
    public abstract class Signal<T1, T2> : ISignal
    {
        private readonly HashSet<Action<T1, T2>> _listeners = new HashSet<Action<T1, T2>>();

        public void AddListener(Action<T1, T2> listener)
        {
            _listeners.Add(listener);
        }

        public void RemoveListener(Action<T1, T2> listener)
        {
            _listeners.Remove(listener);
        }

        public void Invoke(T1 arg0, T2 arg1)
        {
            var listeners = new HashSet<Action<T1, T2>>(_listeners);
            foreach (var listener in listeners)
            {
                listener?.Invoke(arg0, arg1);
            }

            listeners.Clear();
        }
    }
}