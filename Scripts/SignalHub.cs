using System;
using System.Collections.Generic;

namespace AffenSignals
{
    public sealed class SignalHub<T> where T : struct, ISignal
    {
        private static readonly HashSet<Action> RegisteredListeners = new();
        private static readonly HashSet<Action<T>> RegisteredListenersWithArg = new();

        public void AddListener(Action listener)
        {
            RegisteredListeners.Add(listener);
        }
        public void AddListener(Action<T> listener)
        {
            RegisteredListenersWithArg.Add(listener);
        }

        public void RemoveListener(Action listener)
        {
            RegisteredListeners.Remove(listener);
        }

        public void RemoveListener(Action<T> listener)
        {
            RegisteredListenersWithArg.Remove(listener);
        }

        public void Invoke(T arg)
        {
            foreach (var registeredListener in RegisteredListeners)
            {
                registeredListener?.Invoke();
            }
            
            foreach (var registeredListener in RegisteredListenersWithArg)
            {
                registeredListener?.Invoke(arg);
            }
        }
    }
}