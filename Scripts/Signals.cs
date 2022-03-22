using System;
using System.Collections.Generic;

namespace AffenSignals
{
    public static class Signals<T> where T : struct, ISignal
    {
        private static readonly SignalHub<T> DefaultHub = new();

        public static void AddListener(Action listener)
        {
            DefaultHub.AddListener(listener);
        }
        
        public static void AddListener(Action<T> listener)
        {
            DefaultHub.AddListener(listener);
        }

        public static void RemoveListener(Action listener)
        {
            DefaultHub.RemoveListener(listener);
        }

        public static void RemoveListener(Action<T> listener)
        {
            DefaultHub.RemoveListener(listener);
        }

        public static void Invoke(T arg)
        {
            DefaultHub.Invoke(arg);
        }
    }
}