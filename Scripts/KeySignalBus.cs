using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AleVerDes.Signals
{
    public class KeySignalBus<TKey>
    {
        private readonly Dictionary<TKey, Dictionary<Type, List<object>>> _receivers = new();
        private readonly Dictionary<TKey, Dictionary<Type, HashSet<object>>> _toRemove = new();

        private bool _requiredCleanUp;
        
        public void Invoke<TSignal>(TKey key, TSignal signal)
        {
            CleanUp();
            
            if (!_receivers.TryGetValue(key, out var dictionary))
                return;

            if (!dictionary.TryGetValue(typeof(TSignal), out var receiverList))
                return;
            
            foreach (var receiver in receiverList.OfType<SignalReceivedDelegate<TSignal>>())
                try
                {
                    if (_toRemove.TryGetValue(key, out var toRemove) 
                        && toRemove.TryGetValue(typeof(TSignal), out var toRemoveReceivers) 
                        && toRemoveReceivers.Contains(receiver))
                        continue;
                    receiver.Invoke(signal);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
        }

        public void Subscribe<TSignal>(TKey key, SignalReceivedDelegate<TSignal> receiver)
        {
            if (!_receivers.TryGetValue(key, out var dictionary))
                _receivers[key] = dictionary = new Dictionary<Type, List<object>>();
            
            if (!dictionary.TryGetValue(typeof(TSignal), out var receivers))
                dictionary[typeof(TSignal)] = receivers = new List<object>();
            
            receivers.Add(receiver);
        }

        public void Unsubscribe<TSignal>(TKey key, SignalReceivedDelegate<TSignal> receiver)
        {
            if (!_toRemove.TryGetValue(key, out var toRemove))
                return;
            if (!toRemove.TryGetValue(typeof(TSignal), out var toRemoveReceivers))
                toRemove[typeof(TSignal)] = toRemoveReceivers = new HashSet<object>();
            toRemoveReceivers.Add(receiver);
            _requiredCleanUp = true;
        }
        
        private void CleanUp()
        {
            if (!_requiredCleanUp)
                return;

            _requiredCleanUp = false;
            
            foreach (var key in _toRemove.Keys)
            {
                var toRemove = _toRemove[key];
                foreach (var type in toRemove.Keys)
                {
                    if (!_receivers.TryGetValue(key, out var receivers))
                        continue;
                    if (!receivers.TryGetValue(type, out var receiversList))
                        continue;
                    
                    foreach (var toRemoveReceiver in toRemove[type])
                        receiversList.Remove(toRemoveReceiver);
                    
                    toRemove[type].Clear();
                }
            }
        }
    }
}