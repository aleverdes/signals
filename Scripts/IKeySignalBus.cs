using System;

namespace AleVerDes.Signals
{
    /// <summary>
    /// Interface for a keyed signal bus that manages typed signals scoped by keys.
    /// Provides methods for subscribing to, unsubscribing from, and invoking signals within specific key scopes.
    /// </summary>
    /// <typeparam name="TKey">The type of key used to scope signals.</typeparam>
    public interface IKeySignalBus<TKey>
    {
        /// <summary>
        /// Subscribes a receiver to a specific signal type within a key scope.
        /// When the signal is invoked for the specified key, the receiver will be called with the signal data.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to subscribe to.</typeparam>
        /// <param name="key">The key that scopes this subscription.</param>
        /// <param name="receiver">The delegate that will handle the signal when invoked.</param>
        /// <exception cref="ArgumentNullException">Thrown when receiver is null.</exception>
        void Subscribe<TSignal>(TKey key, SignalReceivedDelegate<TSignal> receiver);

        /// <summary>
        /// Unsubscribes a receiver from a specific signal type within a key scope.
        /// The receiver will no longer be called when the signal is invoked for the specified key.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to unsubscribe from.</typeparam>
        /// <param name="key">The key that scopes this unsubscription.</param>
        /// <param name="receiver">The delegate to remove from subscribers.</param>
        /// <exception cref="ArgumentNullException">Thrown when receiver is null.</exception>
        void Unsubscribe<TSignal>(TKey key, SignalReceivedDelegate<TSignal> receiver);

        /// <summary>
        /// Invokes a signal within a specific key scope, notifying all subscribers for that key and signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to invoke.</typeparam>
        /// <param name="key">The key that scopes this signal invocation.</param>
        /// <param name="signal">The signal data to pass to subscribers.</param>
        void Invoke<TSignal>(TKey key, TSignal signal);

        /// <summary>
        /// Gets the number of subscribers for a specific signal type within a key scope.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to check.</typeparam>
        /// <param name="key">The key that scopes the check.</param>
        /// <returns>The number of active subscribers for the signal type within the key scope.</returns>
        int GetSubscriberCount<TSignal>(TKey key);

        /// <summary>
        /// Checks if there are any subscribers for a specific signal type within a key scope.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to check.</typeparam>
        /// <param name="key">The key that scopes the check.</param>
        /// <returns>True if there are subscribers for the signal type within the key scope, false otherwise.</returns>
        bool HasSubscribers<TSignal>(TKey key);

        /// <summary>
        /// Removes all subscribers for all signal types within a specific key scope.
        /// </summary>
        /// <param name="key">The key that scopes the clear operation.</param>
        void Clear(TKey key);

        /// <summary>
        /// Removes all subscribers for a specific signal type within a specific key scope.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to clear subscribers for.</typeparam>
        /// <param name="key">The key that scopes the clear operation.</param>
        void Clear<TSignal>(TKey key);

        /// <summary>
        /// Removes all subscribers for all keys and all signal types.
        /// Use with caution as this will clear all subscriptions.
        /// </summary>
        void ClearAll();

        /// <summary>
        /// Gets all keys that have active subscriptions.
        /// </summary>
        /// <returns>An array of keys that have at least one subscription.</returns>
        TKey[] GetActiveKeys();
    }
}
