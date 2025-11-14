using UnityEngine;

namespace AleVerDes.Signals
{
    /// <summary>
    /// A Unity CustomYieldInstruction that waits for a specific signal to be invoked.
    /// Can be used in coroutines to pause execution until a signal is received.
    /// </summary>
    /// <typeparam name="TSignal">The type of signal to wait for.</typeparam>
    /// <example>
    /// <code>
    /// private IEnumerator MyCoroutine()
    /// {
    ///     // Wait for any signal of type MySignal
    ///     yield return new WaitForSignal&lt;MySignal&gt;(_signalBus);
    ///
    ///     // Wait for signal and handle it with callback
    ///     yield return new WaitForSignal&lt;MySignal&gt;(_signalBus, signal => {
    ///         Debug.Log($"Received signal: {signal.Message}");
    ///     });
    /// }
    /// </code>
    /// </example>
    public class WaitForSignal<TSignal> : CustomYieldInstruction
    {
        /// <summary>
        /// Gets whether the coroutine should keep waiting for the signal.
        /// </summary>
        public override bool keepWaiting => _keepWaiting;
        private bool _keepWaiting;

        private readonly ISignalBus _signalBus;
        private readonly SignalReceivedDelegate<TSignal> _callback;

        /// <summary>
        /// Creates a new WaitForSignal instruction that waits for the specified signal type.
        /// </summary>
        /// <param name="signalBus">The signal bus to listen on.</param>
        /// <param name="callback">Optional callback to handle the signal when received.</param>
        /// <exception cref="ArgumentNullException">Thrown when signalBus is null.</exception>
        public WaitForSignal(ISignalBus signalBus, SignalReceivedDelegate<TSignal> callback = null)
        {
            _signalBus = signalBus ?? throw new ArgumentNullException(nameof(signalBus));
            _keepWaiting = true;
            _callback = callback;
            _signalBus.Subscribe<TSignal>(Listener);
        }

        /// <summary>
        /// Internal listener that handles signal reception.
        /// Automatically unsubscribes after receiving the signal.
        /// </summary>
        /// <param name="signal">The received signal data.</param>
        private void Listener(TSignal signal)
        {
            _keepWaiting = false;
            _signalBus.Unsubscribe<TSignal>(Listener);
            _callback?.Invoke(signal);
        }
    }
}