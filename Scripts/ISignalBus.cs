using System;

namespace AleVerDes.Signals
{
    /// <summary>
    /// Interface for a signal bus that manages typed signals and their subscribers.
    /// Provides methods for subscribing to, unsubscribing from, and invoking signals.
    /// </summary>
    public interface ISignalBus
    {
        /// <summary>
        /// Subscribes a receiver to a specific signal type.
        /// When the signal is invoked, the receiver will be called with the signal data.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to subscribe to.</typeparam>
        /// <param name="receiver">The delegate that will handle the signal when invoked.</param>
        /// <exception cref="ArgumentNullException">Thrown when receiver is null.</exception>
        void Subscribe<TSignal>(SignalReceivedDelegate<TSignal> receiver);

        /// <summary>
        /// Unsubscribes a receiver from a specific signal type.
        /// The receiver will no longer be called when the signal is invoked.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to unsubscribe from.</typeparam>
        /// <param name="receiver">The delegate to remove from subscribers.</param>
        /// <exception cref="ArgumentNullException">Thrown when receiver is null.</exception>
        void Unsubscribe<TSignal>(SignalReceivedDelegate<TSignal> receiver);

        /// <summary>
        /// Invokes a signal, notifying all subscribers of the specified signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to invoke.</typeparam>
        /// <param name="signal">The signal data to pass to subscribers.</param>
        void Invoke<TSignal>(TSignal signal);

        /// <summary>
        /// Gets the number of subscribers for a specific signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to check.</typeparam>
        /// <returns>The number of active subscribers for the signal type.</returns>
        int GetSubscriberCount<TSignal>();

        /// <summary>
        /// Checks if there are any subscribers for a specific signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to check.</typeparam>
        /// <returns>True if there are subscribers for the signal type, false otherwise.</returns>
        bool HasSubscribers<TSignal>();

        /// <summary>
        /// Removes all subscribers for all signal types.
        /// Use with caution as this will clear all subscriptions.
        /// </summary>
        void ClearAll();

        /// <summary>
        /// Removes all subscribers for a specific signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to clear subscribers for.</typeparam>
        void Clear<TSignal>();
    }
}
