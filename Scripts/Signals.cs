using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AffenSignals
{
    public static class Signals<T> where T : struct, ISignal
    {
        private static readonly SignalHub<T> DefaultHub = new();
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static void AddListener(Action listener)
        {
            DefaultHub.AddListener(listener);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static void AddListener(Action<T> listener)
        {
            DefaultHub.AddListener(listener);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static void RemoveListener(Action listener)
        {
            DefaultHub.RemoveListener(listener);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static void RemoveListener(Action<T> listener)
        {
            DefaultHub.RemoveListener(listener);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static void Invoke(T arg)
        {
            DefaultHub.Invoke(arg);
        }
    }
}