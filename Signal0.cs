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

        public void Invoke()
        {
            var listeners = new HashSet<Action>(_listeners);
            foreach (Action listener in listeners)
            {
                listener?.Invoke();
            }

            listeners.Clear();
        }
    }

    public class WaitForSignal<T> : UnityEngine.CustomYieldInstruction where T : Signal, new()
    {
        public override bool keepWaiting => _keepWaiting;
        private bool _keepWaiting;

        public WaitForSignal()
        {
            _keepWaiting = true;
            Signals.Get<T>().AddListener(Listener);
        }

        private void Listener()
        {
            _keepWaiting = false;
            Signals.Get<T>().RemoveListener(Listener);
        }
    }
}