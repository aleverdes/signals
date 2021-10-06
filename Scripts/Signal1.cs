using System;
using System.Collections.Generic;

namespace AffenSignals
{
    public abstract class Signal<T> : ISignal
    {
        private readonly HashSet<Action> _listenersWithoutParameters = new HashSet<Action>();
        private readonly HashSet<Action<T>> _listeners = new HashSet<Action<T>>();

        public void AddListener(Action listener)
        {
            _listenersWithoutParameters.Add(listener);
        }

        public void AddListener(Action<T> listener)
        {
            _listeners.Add(listener);
        }

        public void RemoveListener(Action listener)
        {
            _listenersWithoutParameters.Remove(listener);
        }

        public void RemoveListener(Action<T> listener)
        {
            _listeners.Remove(listener);
        }

        public void RemoveAllListeners()
        {
            _listeners.Clear();
            _listenersWithoutParameters.Clear();
        }

        public void Invoke(T arg0)
        {
            var listeners = new HashSet<Action<T>>(_listeners);
            foreach (var listener in listeners)
            {
                listener?.Invoke(arg0);
            }

            listeners.Clear();

            var listenersWithoutParameters = new HashSet<Action>(_listenersWithoutParameters);
            foreach (var listener in listenersWithoutParameters)
            {
                listener?.Invoke();
            }

            listenersWithoutParameters.Clear();
        }
    }
}