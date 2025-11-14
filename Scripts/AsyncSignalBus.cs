using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace AleVerDes.Signals
{
    /// <summary>
    /// A signal bus that supports asynchronous signal handlers using async/await.
    /// </summary>
    /// <remarks>
    /// Async handlers are executed concurrently, allowing for non-blocking signal processing.
    /// Exceptions in individual handlers are caught and logged, but don't prevent other handlers from executing.
    /// </remarks>
    public class AsyncSignalBus : ISignalBus
    {
        private readonly Dictionary<Type, List<object>> _receivers = new();
        private readonly Dictionary<Type, HashSet<object>> _toRemove = new();

        private bool _requiredCleanUp;

        /// <summary>
        /// Delegate type for asynchronous signal handlers.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal data.</typeparam>
        /// <param name="signal">The signal data instance.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public delegate Task AsyncSignalReceivedDelegate<in TSignal>(TSignal signal);

        /// <summary>
        /// Subscribes an asynchronous receiver to a specific signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to subscribe to.</typeparam>
        /// <param name="receiver">The asynchronous delegate that will handle the signal when invoked.</param>
        /// <exception cref="ArgumentNullException">Thrown when receiver is null.</exception>
        public void Subscribe<TSignal>(AsyncSignalReceivedDelegate<TSignal> receiver)
        {
            if (receiver == null)
                throw new ArgumentNullException(nameof(receiver));

            if (!_receivers.TryGetValue(typeof(TSignal), out var receivers))
                _receivers[typeof(TSignal)] = receivers = new List<object>();
            receivers.Add(receiver);
        }

        /// <summary>
        /// Subscribes a synchronous receiver to a specific signal type.
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
        /// Unsubscribes an asynchronous receiver from a specific signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to unsubscribe from.</typeparam>
        /// <param name="receiver">The asynchronous delegate to remove from subscribers.</param>
        /// <exception cref="ArgumentNullException">Thrown when receiver is null.</exception>
        public void Unsubscribe<TSignal>(AsyncSignalReceivedDelegate<TSignal> receiver)
        {
            if (receiver == null)
                throw new ArgumentNullException(nameof(receiver));

            if (!_toRemove.TryGetValue(typeof(TSignal), out var toRemove))
                _toRemove[typeof(TSignal)] = toRemove = new HashSet<object>();
            toRemove.Add(receiver);
            _requiredCleanUp = true;
        }

        /// <summary>
        /// Asynchronously invokes a signal, notifying all subscribers of the specified signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to invoke.</typeparam>
        /// <param name="signal">The signal data to pass to subscribers.</param>
        /// <returns>A task that completes when all signal handlers have finished executing.</returns>
        public async Task InvokeAsync<TSignal>(TSignal signal)
        {
            CleanUp();

            if (!_receivers.TryGetValue(typeof(TSignal), out var receivers))
                return;

            var tasks = new List<Task>();

            foreach (var receiver in receivers)
            {
                if (_toRemove.TryGetValue(typeof(TSignal), out var toRemove) && toRemove.Contains(receiver))
                    continue;

                try
                {
                    if (receiver is AsyncSignalReceivedDelegate<TSignal> asyncReceiver)
                    {
                        tasks.Add(asyncReceiver(signal));
                    }
                    else if (receiver is SignalReceivedDelegate<TSignal> syncReceiver)
                    {
                        tasks.Add(Task.Run(() => syncReceiver(signal)));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Signal handler threw an exception for signal {typeof(TSignal).Name}: {e}");
                }
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Synchronously invokes a signal, notifying all subscribers of the specified signal type.
        /// This method blocks until all handlers complete, which may impact performance.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to invoke.</typeparam>
        /// <param name="signal">The signal data to pass to subscribers.</param>
        public void Invoke<TSignal>(TSignal signal)
        {
            // For synchronous invocation, we create a task but block on it
            InvokeAsync(signal).Wait();
        }

        /// <summary>
        /// Fire-and-forget signal invocation. Handlers run asynchronously without waiting.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to invoke.</typeparam>
        /// <param name="signal">The signal data to pass to subscribers.</param>
        public void InvokeFireAndForget<TSignal>(TSignal signal)
        {
            // Start the async invocation without waiting
            InvokeAsync(signal);
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
        /// </summary>
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
