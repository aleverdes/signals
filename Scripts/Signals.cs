using System;
using System.Collections.Generic;

namespace AffenSignals
{
    public static class Signals
    {
        private static readonly SignalContainer DefaultContainer = new SignalContainer();
        
        public static T Get<T>() where T : ISignal, new()
        {
            return DefaultContainer.Get<T>();
        }
    }
}