using UnityEngine;

namespace AffenCode.Signals
{
    public class WaitForSignal<T> : CustomYieldInstruction where T : struct, ISignal
    {
        public override bool keepWaiting => _keepWaiting;
        private bool _keepWaiting;

        public WaitForSignal()
        {
            _keepWaiting = true;
            Signals<T>.AddListener(Listener);
        }

        private void Listener()
        {
            _keepWaiting = false;
            Signals<T>.RemoveListener(Listener);
        }
    }
}