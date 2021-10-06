using System;

namespace AffenSignals
{
    public interface ISignal
    {
        void AddListener(Action listener);
        void RemoveListener(Action listener);
    }
}