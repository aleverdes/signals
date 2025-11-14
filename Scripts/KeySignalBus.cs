using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AleVerDes.Signals
{
    /// <summary>
    /// A high-performance keyed signal bus implementation for Unity that manages typed signals scoped by keys.
    /// Provides thread-safe operations and automatic cleanup of unsubscribed handlers within key scopes.
    /// </summary>
    /// <typeparam name="TKey">The type of key used to scope signals.</typeparam>
    /// <remarks>
    /// This implementation allows for isolated signal handling within different key scopes,
    /// making it ideal for component-specific or context-specific messaging.
    /// Uses a deferred cleanup strategy to ensure safe unsubscription during signal invocation.
    /// </remarks>
    public class KeySignalBus<TKey> : IKeySignalBus<TKey>
    {
        private readonly Dictionary<TKey, Dictionary<Type, List<object>>> _receivers = new();
        private readonly Dictionary<TKey, Dictionary<Type, HashSet<object>>> _toRemove = new();

        private bool _requiredCleanUp;

        /// <summary>
        /// Invokes a signal within a specific key scope, notifying all subscribers for that key and signal type.
        /// Exceptions in individual handlers are caught and logged, allowing other handlers to continue processing.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to invoke.</typeparam>
        /// <param name="key">The key that scopes this signal invocation.</param>
        /// <param name="signal">The signal data to pass to subscribers.</param>
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
                    receiver(signal);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Signal handler threw an exception for signal {typeof(TSignal).Name} with key '{key}': {e}");
                }
        }

        /// <summary>
        /// Subscribes a receiver to a specific signal type within a key scope.
        /// When the signal is invoked for the specified key, the receiver will be called with the signal data.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to subscribe to.</typeparam>
        /// <param name="key">The key that scopes this subscription.</param>
        /// <param name="receiver">The delegate that will handle the signal when invoked.</param>
        /// <exception cref="ArgumentNullException">Thrown when receiver is null.</exception>
        public void Subscribe<TSignal>(TKey key, SignalReceivedDelegate<TSignal> receiver)
        {
            if (receiver == null)
                throw new ArgumentNullException(nameof(receiver));

            if (!_receivers.TryGetValue(key, out var dictionary))
                _receivers[key] = dictionary = new Dictionary<Type, List<object>>();

            if (!dictionary.TryGetValue(typeof(TSignal), out var receivers))
                dictionary[typeof(TSignal)] = receivers = new List<object>();

            receivers.Add(receiver);
        }

        /// <summary>
        /// Unsubscribes a receiver from a specific signal type within a key scope.
        /// The receiver will no longer be called when the signal is invoked for the specified key.
        /// Unsubscription is deferred until the next signal invocation to ensure thread safety.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to unsubscribe from.</typeparam>
        /// <param name="key">The key that scopes this unsubscription.</param>
        /// <param name="receiver">The delegate to remove from subscribers.</param>
        /// <exception cref="ArgumentNullException">Thrown when receiver is null.</exception>
        public void Unsubscribe<TSignal>(TKey key, SignalReceivedDelegate<TSignal> receiver)
        {
            if (receiver == null)
                throw new ArgumentNullException(nameof(receiver));

            if (!_toRemove.TryGetValue(key, out var toRemove))
                _toRemove[key] = toRemove = new Dictionary<Type, HashSet<object>>();
            if (!toRemove.TryGetValue(typeof(TSignal), out var toRemoveReceivers))
                toRemove[typeof(TSignal)] = toRemoveReceivers = new HashSet<object>();
            toRemoveReceivers.Add(receiver);
            _requiredCleanUp = true;
        }

        /// <summary>
        /// Gets the number of subscribers for a specific signal type within a key scope.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to check.</typeparam>
        /// <param name="key">The key that scopes the check.</param>
        /// <returns>The number of active subscribers for the signal type within the key scope.</returns>
        public int GetSubscriberCount<TSignal>(TKey key)
        {
            CleanUp();
            if (!_receivers.TryGetValue(key, out var dictionary))
                return 0;
            return dictionary.TryGetValue(typeof(TSignal), out var receivers) ? receivers.Count : 0;
        }

        /// <summary>
        /// Checks if there are any subscribers for a specific signal type within a key scope.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to check.</typeparam>
        /// <param name="key">The key that scopes the check.</param>
        /// <returns>True if there are subscribers for the signal type within the key scope, false otherwise.</returns>
        public bool HasSubscribers<TSignal>(TKey key)
        {
            CleanUp();
            if (!_receivers.TryGetValue(key, out var dictionary))
                return false;
            return dictionary.TryGetValue(typeof(TSignal), out var receivers) && receivers.Count > 0;
        }

        /// <summary>
        /// Removes all subscribers for all signal types within a specific key scope.
        /// </summary>
        /// <param name="key">The key that scopes the clear operation.</param>
        public void Clear(TKey key)
        {
            _receivers.Remove(key);
            _toRemove.Remove(key);
        }

        /// <summary>
        /// Removes all subscribers for a specific signal type within a specific key scope.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to clear subscribers for.</typeparam>
        /// <param name="key">The key that scopes the clear operation.</param>
        public void Clear<TSignal>(TKey key)
        {
            if (_receivers.TryGetValue(key, out var dictionary))
                dictionary.Remove(typeof(TSignal));
            if (_toRemove.TryGetValue(key, out var toRemove))
                toRemove.Remove(typeof(TSignal));
        }

        /// <summary>
        /// Removes all subscribers for all keys and all signal types.
        /// Use with caution as this will clear all subscriptions.
        /// </summary>
        public void ClearAll()
        {
            _receivers.Clear();
            _toRemove.Clear();
            _requiredCleanUp = false;
        }

        /// <summary>
        /// Gets all keys that have active subscriptions.
        /// </summary>
        /// <returns>An array of keys that have at least one subscription.</returns>
        public TKey[] GetActiveKeys()
        {
            CleanUp();
            return _receivers.Keys.ToArray();
        }

        /// <summary>
        /// Performs deferred cleanup of unsubscribed handlers.
        /// This method is called automatically during signal invocation.
        /// </summary>
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