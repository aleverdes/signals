using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AleVerDes.Signals
{
    public class SignalBus
    {
        private readonly Dictionary<Type, List<object>> _receivers = new();
        private readonly Dictionary<Type, HashSet<object>> _toRemove = new();

        private bool _requiredCleanUp;
        
        public void Invoke<TSignal>(TSignal signal)
        {
            CleanUp();
            
            if (!_receivers.TryGetValue(typeof(TSignal), out var receivers))
                return;
            
            foreach (var receiver in receivers.OfType<SignalReceivedDelegate<TSignal>>())
                try
                {
                    if (_toRemove.TryGetValue(typeof(TSignal), out var toRemove) && toRemove.Contains(receiver))
                        continue;
                    receiver(signal);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
        }

        public void Subscribe<TSignal>(SignalReceivedDelegate<TSignal> receiver)
        {
            if (!_receivers.TryGetValue(typeof(TSignal), out var receivers))
                _receivers[typeof(TSignal)] = receivers = new List<object>();
            receivers.Add(receiver);
        }

        public void Unsubscribe<TSignal>(SignalReceivedDelegate<TSignal> receiver)
        {
            if (!_toRemove.TryGetValue(typeof(TSignal), out var toRemove))
                _toRemove[typeof(TSignal)] = toRemove = new HashSet<object>();
            toRemove.Add(receiver);
            _requiredCleanUp = true;
        }
        
        private void CleanUp()
        {
            if (!_requiredCleanUp)
                return;

            _requiredCleanUp = false;
            
            foreach (var type in _toRemove.Keys)
            {
                if (!_receivers.TryGetValue(type, out var receivers))
                    continue;
                foreach (var toRemoveReceiver in _toRemove[type])
                    receivers.Remove(toRemoveReceiver);
                
                _toRemove[type].Clear();
            }
        }
    }
}