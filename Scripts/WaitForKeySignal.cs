using UnityEngine;

namespace AleVerDes.Signals
{
    /// <summary>
    /// A Unity CustomYieldInstruction that waits for a specific keyed signal to be invoked.
    /// Can be used in coroutines to pause execution until a signal with a specific key is received.
    /// </summary>
    /// <typeparam name="TKey">The type of key that scopes the signal.</typeparam>
    /// <typeparam name="TSignal">The type of signal to wait for.</typeparam>
    /// <example>
    /// <code>
    /// private IEnumerator MyCoroutine()
    /// {
    ///     // Wait for any keyed signal of type MySignal with key "player1"
    ///     yield return new WaitForKeySignal&lt;string, MySignal&gt;(_keySignalBus, "player1");
    ///
    ///     // Wait for keyed signal and handle it with callback
    ///     yield return new WaitForKeySignal&lt;int, MySignal&gt;(_keySignalBus, 42, signal => {
    ///         Debug.Log($"Received signal for key 42: {signal.Message}");
    ///     });
    /// }
    /// </code>
    /// </example>
    public class WaitForKeySignal<TKey, TSignal> : CustomYieldInstruction
    {
        /// <summary>
        /// Gets whether the coroutine should keep waiting for the keyed signal.
        /// </summary>
        public override bool keepWaiting => _keepWaiting;
        private bool _keepWaiting;

        private readonly TKey _key;
        private readonly IKeySignalBus<TKey> _keySignalBus;
        private readonly SignalReceivedDelegate<TSignal> _callback;

        /// <summary>
        /// Creates a new WaitForKeySignal instruction that waits for the specified keyed signal.
        /// </summary>
        /// <param name="keySignalBus">The keyed signal bus to listen on.</param>
        /// <param name="key">The key that scopes the signal to wait for.</param>
        /// <param name="callback">Optional callback to handle the signal when received.</param>
        /// <exception cref="ArgumentNullException">Thrown when keySignalBus is null.</exception>
        public WaitForKeySignal(IKeySignalBus<TKey> keySignalBus, TKey key, SignalReceivedDelegate<TSignal> callback = null)
        {
            _keySignalBus = keySignalBus ?? throw new ArgumentNullException(nameof(keySignalBus));
            _keepWaiting = true;
            _key = key;
            _callback = callback;
            _keySignalBus.Subscribe<TSignal>(key, Listener);
        }

        /// <summary>
        /// Internal listener that handles keyed signal reception.
        /// Automatically unsubscribes after receiving the signal.
        /// </summary>
        /// <param name="signal">The received signal data.</param>
        private void Listener(TSignal signal)
        {
            _keepWaiting = false;
            _keySignalBus.Unsubscribe<TSignal>(_key, Listener);
            _callback?.Invoke(signal);
        }
    }
}