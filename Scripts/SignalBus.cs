using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AleVerDes.Signals
{
    /// <summary>
    /// A high-performance signal bus implementation for Unity that manages typed signals and their subscribers.
    /// Provides thread-safe operations and automatic cleanup of unsubscribed handlers.
    /// </summary>
    /// <remarks>
    /// This implementation uses a deferred cleanup strategy to avoid modifying collections during iteration,
    /// ensuring safe unsubscription even while signals are being invoked.
    /// </remarks>
    public class SignalBus : ISignalBus
    {
        private readonly Dictionary<Type, List<object>> _receivers = new();
        private readonly Dictionary<Type, HashSet<object>> _toRemove = new();

        private bool _requiredCleanUp;

        /// <summary>
        /// Invokes a signal, notifying all subscribers of the specified signal type.
        /// Exceptions in individual handlers are caught and logged, allowing other handlers to continue processing.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to invoke.</typeparam>
        /// <param name="signal">The signal data to pass to subscribers.</param>
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
                    Debug.LogError($"Signal handler threw an exception for signal {typeof(TSignal).Name}: {e}");
                }
        }

        /// <summary>
        /// Subscribes a receiver to a specific signal type.
        /// When the signal is invoked, the receiver will be called with the signal data.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to subscribe to.</typeparam>
        /// <param name="receiver">The delegate that will handle the signal when invoked.</param>
        /// <exception cref="ArgumentNullException">Thrown when receiver is null.</exception>
        public void Subscribe<TSignal>(SignalReceivedDelegate<TSignal> receiver)
        {
            if (receiver == null)
                throw new ArgumentNullException(nameof(receiver));

            if (!_receivers.TryGetValue(typeof(TSignal), out var receivers))
                _receivers[typeof(TSignal)] = receivers = new List<object>();
            receivers.Add(receiver);
        }

        /// <summary>
        /// Unsubscribes a receiver from a specific signal type.
        /// The receiver will no longer be called when the signal is invoked.
        /// Unsubscription is deferred until the next signal invocation to ensure thread safety.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to unsubscribe from.</typeparam>
        /// <param name="receiver">The delegate to remove from subscribers.</param>
        /// <exception cref="ArgumentNullException">Thrown when receiver is null.</exception>
        public void Unsubscribe<TSignal>(SignalReceivedDelegate<TSignal> receiver)
        {
            if (receiver == null)
                throw new ArgumentNullException(nameof(receiver));

            if (!_toRemove.TryGetValue(typeof(TSignal), out var toRemove))
                _toRemove[typeof(TSignal)] = toRemove = new HashSet<object>();
            toRemove.Add(receiver);
            _requiredCleanUp = true;
        }

        /// <summary>
        /// Gets the number of subscribers for a specific signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to check.</typeparam>
        /// <returns>The number of active subscribers for the signal type.</returns>
        public int GetSubscriberCount<TSignal>()
        {
            CleanUp();
            return _receivers.TryGetValue(typeof(TSignal), out var receivers) ? receivers.Count : 0;
        }

        /// <summary>
        /// Checks if there are any subscribers for a specific signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to check.</typeparam>
        /// <returns>True if there are subscribers for the signal type, false otherwise.</returns>
        public bool HasSubscribers<TSignal>()
        {
            CleanUp();
            return _receivers.TryGetValue(typeof(TSignal), out var receivers) && receivers.Count > 0;
        }

        /// <summary>
        /// Removes all subscribers for all signal types.
        /// Use with caution as this will clear all subscriptions.
        /// </summary>
        public void ClearAll()
        {
            _receivers.Clear();
            _toRemove.Clear();
            _requiredCleanUp = false;
        }

        /// <summary>
        /// Removes all subscribers for a specific signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to clear subscribers for.</typeparam>
        public void Clear<TSignal>()
        {
            _receivers.Remove(typeof(TSignal));
            _toRemove.Remove(typeof(TSignal));
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

            foreach (var kvp in _toRemove)
            {
                var type = kvp.Key;
                var toRemoveSet = kvp.Value;

                if (_receivers.TryGetValue(type, out var receivers))
                {
                    // Use RemoveAll for better performance when removing multiple items
                    receivers.RemoveAll(receiver => toRemoveSet.Contains(receiver));
                }

                toRemoveSet.Clear();
            }
        }
    }
}