using System;
using System.Collections.Generic;

namespace AffenSignals
{
    public sealed class SignalContainer
    {
        private readonly Dictionary<Type, ISignal> _registeredSignals = new Dictionary<Type, ISignal>();

        public T Get<T>() where T : ISignal, new()
        {
            if (!_registeredSignals.TryGetValue(typeof(T), out ISignal signal))
            {
                signal = new T();
                _registeredSignals.Add(typeof(T), signal);
            }

            return (T) signal;
        }
    }
}