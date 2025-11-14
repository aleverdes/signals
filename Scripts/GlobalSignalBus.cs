using UnityEngine;

namespace AleVerDes.Signals
{
    /// <summary>
    /// A ScriptableObject-based global signal bus that can be easily configured in Unity scenes.
    /// Provides a singleton-like access pattern for global messaging throughout your application.
    /// </summary>
    /// <remarks>
    /// Create an instance of this asset in your Resources folder or reference it directly in your scenes.
    /// Multiple instances can exist for different scopes (e.g., one for UI signals, one for gameplay signals).
    /// </remarks>
    [CreateAssetMenu(fileName = "GlobalSignalBus", menuName = "AleVerDes/Signals/Global Signal Bus", order = 1)]
    public class GlobalSignalBus : ScriptableObject, ISignalBus
    {
        [SerializeField, Tooltip("Optional description for this signal bus instance")]
        private string _description;

        // Internal signal bus implementation
        private SignalBus _signalBus;

        /// <summary>
        /// Gets the description of this signal bus instance.
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// Initializes the internal signal bus when the asset is loaded.
        /// </summary>
        private void OnEnable()
        {
            if (_signalBus == null)
            {
                _signalBus = new SignalBus();
            }
        }

        /// <summary>
        /// Subscribes a receiver to a specific signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to subscribe to.</typeparam>
        /// <param name="receiver">The delegate that will handle the signal when invoked.</param>
        /// <exception cref="ArgumentNullException">Thrown when receiver is null.</exception>
        public void Subscribe<TSignal>(SignalReceivedDelegate<TSignal> receiver)
        {
            EnsureInitialized();
            _signalBus.Subscribe(receiver);
        }

        /// <summary>
        /// Unsubscribes a receiver from a specific signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to unsubscribe from.</typeparam>
        /// <param name="receiver">The delegate to remove from subscribers.</param>
        /// <exception cref="ArgumentNullException">Thrown when receiver is null.</exception>
        public void Unsubscribe<TSignal>(SignalReceivedDelegate<TSignal> receiver)
        {
            EnsureInitialized();
            _signalBus.Unsubscribe(receiver);
        }

        /// <summary>
        /// Invokes a signal, notifying all subscribers of the specified signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to invoke.</typeparam>
        /// <param name="signal">The signal data to pass to subscribers.</param>
        public void Invoke<TSignal>(TSignal signal)
        {
            EnsureInitialized();
            _signalBus.Invoke(signal);
        }

        /// <summary>
        /// Gets the number of subscribers for a specific signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to check.</typeparam>
        /// <returns>The number of active subscribers for the signal type.</returns>
        public int GetSubscriberCount<TSignal>()
        {
            EnsureInitialized();
            return _signalBus.GetSubscriberCount<TSignal>();
        }

        /// <summary>
        /// Checks if there are any subscribers for a specific signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to check.</typeparam>
        /// <returns>True if there are subscribers for the signal type, false otherwise.</returns>
        public bool HasSubscribers<TSignal>()
        {
            EnsureInitialized();
            return _signalBus.HasSubscribers<TSignal>();
        }

        /// <summary>
        /// Removes all subscribers for all signal types.
        /// </summary>
        public void ClearAll()
        {
            EnsureInitialized();
            _signalBus.ClearAll();
        }

        /// <summary>
        /// Removes all subscribers for a specific signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to clear subscribers for.</typeparam>
        public void Clear<TSignal>()
        {
            EnsureInitialized();
            _signalBus.Clear<TSignal>();
        }

        /// <summary>
        /// Ensures the internal signal bus is initialized.
        /// </summary>
        private void EnsureInitialized()
        {
            if (_signalBus == null)
            {
                _signalBus = new SignalBus();
            }
        }
    }
}
