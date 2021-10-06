using UnityEngine;

namespace AffenSignals
{
    public class WaitForSignal<T> : CustomYieldInstruction where T : ISignal, new()
    {
        public override bool keepWaiting => _keepWaiting;
        private bool _keepWaiting;

        public WaitForSignal()
        {
            _keepWaiting = true;
            Signals.Get<T>().AddListener(Listener);
        }

        private void Listener()
        {
            _keepWaiting = false;
            Signals.Get<T>().RemoveListener(Listener);
        }
    }
}