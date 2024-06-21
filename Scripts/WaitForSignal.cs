using UnityEngine;

namespace AleVerDes.Signals
{
    public class WaitForSignal<TSignal> : CustomYieldInstruction where TSignal : struct
    {
        public override bool keepWaiting => _keepWaiting;
        private bool _keepWaiting;

        private readonly SignalBus _signalBus;
        private readonly SignalReceivedDelegate<TSignal> _callback;

        public WaitForSignal(SignalBus signalBus, SignalReceivedDelegate<TSignal> callback = null)
        {
            _keepWaiting = true;
            _signalBus = signalBus;
            _callback = callback;
            _signalBus.Subscribe<TSignal>(Listener);
        }

        private void Listener(TSignal signal)
        {
            _keepWaiting = false;
            _signalBus.Unsubscribe<TSignal>(Listener);
            _callback?.Invoke(signal);
        }
    }
}