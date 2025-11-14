using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AleVerDes.Signals
{
    /// <summary>
    /// A signal bus that supports prioritized signal handlers.
    /// Handlers with higher priority values are executed before handlers with lower priority values.
    /// </summary>
    /// <remarks>
    /// Priorities are integers where higher values indicate higher priority (executed first).
    /// Default priority is 0. Negative priorities are allowed.
    /// </remarks>
    public class PrioritizedSignalBus : ISignalBus
    {
        private readonly Dictionary<Type, SortedDictionary<int, List<object>>> _receivers = new();
        private readonly Dictionary<Type, HashSet<object>> _toRemove = new();

        private bool _requiredCleanUp;

        /// <summary>
        /// Subscribes a receiver to a specific signal type with a given priority.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to subscribe to.</typeparam>
        /// <param name="receiver">The delegate that will handle the signal when invoked.</param>
        /// <param name="priority">The priority of this handler (higher values = higher priority).</param>
        /// <exception cref="ArgumentNullException">Thrown when receiver is null.</exception>
        public void Subscribe<TSignal>(SignalReceivedDelegate<TSignal> receiver, int priority = 0)
        {
            if (receiver == null)
                throw new ArgumentNullException(nameof(receiver));

            if (!_receivers.TryGetValue(typeof(TSignal), out var priorityDict))
            {
                priorityDict = new SortedDictionary<int, List<object>>(Comparer<int>.Create((x, y) => y.CompareTo(x))); // Higher priority first
                _receivers[typeof(TSignal)] = priorityDict;
            }

            if (!priorityDict.TryGetValue(priority, out var receivers))
            {
                receivers = new List<object>();
                priorityDict[priority] = receivers;
            }

            receivers.Add(receiver);
        }

        /// <summary>
        /// Subscribes a receiver to a specific signal type with default priority (0).
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to subscribe to.</typeparam>
        /// <param name="receiver">The delegate that will handle the signal when invoked.</param>
        /// <exception cref="ArgumentNullException">Thrown when receiver is null.</exception>
        public void Subscribe<TSignal>(SignalReceivedDelegate<TSignal> receiver)
        {
            Subscribe(receiver, 0);
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
        /// Invokes a signal, notifying all subscribers of the specified signal type in priority order.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to invoke.</typeparam>
        /// <param name="signal">The signal data to pass to subscribers.</param>
        public void Invoke<TSignal>(TSignal signal)
        {
            CleanUp();

            if (!_receivers.TryGetValue(typeof(TSignal), out var priorityDict))
                return;

            foreach (var priorityLevel in priorityDict)
            {
                foreach (var receiver in priorityLevel.Value.OfType<SignalReceivedDelegate<TSignal>>())
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
        }

        /// <summary>
        /// Gets the number of subscribers for a specific signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to check.</typeparam>
        /// <returns>The number of active subscribers for the signal type.</returns>
        public int GetSubscriberCount<TSignal>()
        {
            CleanUp();
            if (!_receivers.TryGetValue(typeof(TSignal), out var priorityDict))
                return 0;
            return priorityDict.Sum(kvp => kvp.Value.Count);
        }

        /// <summary>
        /// Checks if there are any subscribers for a specific signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to check.</typeparam>
        /// <returns>True if there are subscribers for the signal type, false otherwise.</returns>
        public bool HasSubscribers<TSignal>()
        {
            CleanUp();
            if (!_receivers.TryGetValue(typeof(TSignal), out var priorityDict))
                return false;
            return priorityDict.Any(kvp => kvp.Value.Count > 0);
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
        /// Gets the priority levels for a specific signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to check.</typeparam>
        /// <returns>An array of priority levels that have subscribers.</returns>
        public int[] GetPriorityLevels<TSignal>()
        {
            CleanUp();
            if (!_receivers.TryGetValue(typeof(TSignal), out var priorityDict))
                return Array.Empty<int>();
            return priorityDict.Keys.Where(priority => priorityDict[priority].Count > 0).ToArray();
        }

        /// <summary>
        /// Gets the subscriber count for a specific signal type at a specific priority level.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to check.</typeparam>
        /// <param name="priority">The priority level to check.</param>
        /// <returns>The number of subscribers at the specified priority level.</returns>
        public int GetSubscriberCountAtPriority<TSignal>(int priority)
        {
            CleanUp();
            if (!_receivers.TryGetValue(typeof(TSignal), out var priorityDict))
                return 0;
            return priorityDict.TryGetValue(priority, out var receivers) ? receivers.Count : 0;
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
                if (!_receivers.TryGetValue(type, out var priorityDict))
                    continue;

                foreach (var priorityLevel in priorityDict)
                {
                    foreach (var toRemoveReceiver in _toRemove[type])
                        priorityLevel.Value.Remove(toRemoveReceiver);

                    // Remove empty priority levels
                    if (priorityLevel.Value.Count == 0)
                        priorityDict.Remove(priorityLevel.Key);
                }

                _toRemove[type].Clear();
            }
        }
    }
}
