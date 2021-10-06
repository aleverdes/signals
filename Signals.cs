using System;
using System.Collections.Generic;

namespace AffenSignals
{
    public static class Signals
    {
        private static readonly Dictionary<Type, ISignal> RegisteredSignals = new Dictionary<Type, ISignal>();

        public static T Get<T>() where T : ISignal, new()
        {
            if (!RegisteredSignals.TryGetValue(typeof(T), out ISignal signal))
            {
                signal = (ISignal) Activator.CreateInstance(typeof(T));
                RegisteredSignals.Add(typeof(T), signal);
            }

            return (T) signal;
        }
    }
}