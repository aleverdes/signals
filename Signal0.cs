using System;
using System.Collections.Generic;

namespace AffenSignals
{
    public abstract class Signal : ISignal
    {
        private readonly HashSet<Action> _listeners = new HashSet<Action>();

        public void AddListener(Action listener)
        {
            _listeners.Add(listener);
        }

        public void RemoveListener(Action listener)
        {
            _listeners.Remove(listener);
        }

        public void RemoveAllListeners()
        {
            _listeners.Clear();
        }

        public void Invoke()
        {
            var listeners = new HashSet<Action>(_listeners);
            foreach (var listener in listeners)
            {
                listener?.Invoke();
            }

            listeners.Clear();
        }
    }
}