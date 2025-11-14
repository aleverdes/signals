using UnityEngine;

namespace AleVerDes.Signals
{
    /// <summary>
    /// A ScriptableObject-based global keyed signal bus that can be easily configured in Unity scenes.
    /// Provides a singleton-like access pattern for scoped global messaging throughout your application.
    /// </summary>
    /// <remarks>
    /// Create an instance of this asset in your Resources folder or reference it directly in your scenes.
    /// Useful for component-specific or context-specific global messaging.
    /// </remarks>
    [CreateAssetMenu(fileName = "GlobalKeySignalBus", menuName = "AleVerDes/Signals/Global Key Signal Bus", order = 2)]
    public class GlobalKeySignalBus<TKey> : ScriptableObject, IKeySignalBus<TKey>
    {
        [SerializeField, Tooltip("Optional description for this keyed signal bus instance")]
        private string _description;

        // Internal keyed signal bus implementation
        private KeySignalBus<TKey> _keySignalBus;

        /// <summary>
        /// Gets the description of this keyed signal bus instance.
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// Initializes the internal keyed signal bus when the asset is loaded.
        /// </summary>
        private void OnEnable()
        {
            if (_keySignalBus == null)
            {
                _keySignalBus = new KeySignalBus<TKey>();
            }
        }

        /// <summary>
        /// Subscribes a receiver to a specific signal type within a key scope.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to subscribe to.</typeparam>
        /// <param name="key">The key that scopes this subscription.</param>
        /// <param name="receiver">The delegate that will handle the signal when invoked.</param>
        /// <exception cref="ArgumentNullException">Thrown when receiver is null.</exception>
        public void Subscribe<TSignal>(TKey key, SignalReceivedDelegate<TSignal> receiver)
        {
            EnsureInitialized();
            _keySignalBus.Subscribe(key, receiver);
        }

        /// <summary>
        /// Unsubscribes a receiver from a specific signal type within a key scope.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to unsubscribe from.</typeparam>
        /// <param name="key">The key that scopes this unsubscription.</param>
        /// <param name="receiver">The delegate to remove from subscribers.</param>
        /// <exception cref="ArgumentNullException">Thrown when receiver is null.</exception>
        public void Unsubscribe<TSignal>(TKey key, SignalReceivedDelegate<TSignal> receiver)
        {
            EnsureInitialized();
            _keySignalBus.Unsubscribe(key, receiver);
        }

        /// <summary>
        /// Invokes a signal within a specific key scope, notifying all subscribers for that key and signal type.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to invoke.</typeparam>
        /// <param name="key">The key that scopes this signal invocation.</param>
        /// <param name="signal">The signal data to pass to subscribers.</param>
        public void Invoke<TSignal>(TKey key, TSignal signal)
        {
            EnsureInitialized();
            _keySignalBus.Invoke(key, signal);
        }

        /// <summary>
        /// Gets the number of subscribers for a specific signal type within a key scope.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to check.</typeparam>
        /// <param name="key">The key that scopes the check.</param>
        /// <returns>The number of active subscribers for the signal type within the key scope.</returns>
        public int GetSubscriberCount<TSignal>(TKey key)
        {
            EnsureInitialized();
            return _keySignalBus.GetSubscriberCount<TSignal>(key);
        }

        /// <summary>
        /// Checks if there are any subscribers for a specific signal type within a key scope.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to check.</typeparam>
        /// <param name="key">The key that scopes the check.</param>
        /// <returns>True if there are subscribers for the signal type within the key scope, false otherwise.</returns>
        public bool HasSubscribers<TSignal>(TKey key)
        {
            EnsureInitialized();
            return _keySignalBus.HasSubscribers<TSignal>(key);
        }

        /// <summary>
        /// Removes all subscribers for all signal types within a specific key scope.
        /// </summary>
        /// <param name="key">The key that scopes the clear operation.</param>
        public void Clear(TKey key)
        {
            EnsureInitialized();
            _keySignalBus.Clear(key);
        }

        /// <summary>
        /// Removes all subscribers for a specific signal type within a specific key scope.
        /// </summary>
        /// <typeparam name="TSignal">The type of signal to clear subscribers for.</typeparam>
        /// <param name="key">The key that scopes the clear operation.</param>
        public void Clear<TSignal>(TKey key)
        {
            EnsureInitialized();
            _keySignalBus.Clear<TSignal>(key);
        }

        /// <summary>
        /// Removes all subscribers for all keys and all signal types.
        /// </summary>
        public void ClearAll()
        {
            EnsureInitialized();
            _keySignalBus.ClearAll();
        }

        /// <summary>
        /// Gets all keys that have active subscriptions.
        /// </summary>
        /// <returns>An array of keys that have at least one subscription.</returns>
        public TKey[] GetActiveKeys()
        {
            EnsureInitialized();
            return _keySignalBus.GetActiveKeys();
        }

        /// <summary>
        /// Ensures the internal keyed signal bus is initialized.
        /// </summary>
        private void EnsureInitialized()
        {
            if (_keySignalBus == null)
            {
                _keySignalBus = new KeySignalBus<TKey>();
            }
        }
    }
}
