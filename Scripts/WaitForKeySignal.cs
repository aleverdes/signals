using UnityEngine;

namespace TaigaGames.Signals
{
    public class WaitForKeySignal<TKey, TSignal> : CustomYieldInstruction where TSignal : struct
    {
        public override bool keepWaiting => _keepWaiting;
        private bool _keepWaiting;
        
        private readonly TKey _key;
        private readonly KeySignalBus<TKey> _keySignalBus;
        private readonly SignalReceivedDelegate<TSignal> _callback;

        public WaitForKeySignal(KeySignalBus<TKey> keySignalBus, TKey key, SignalReceivedDelegate<TSignal> callback = null)
        {
            _keepWaiting = true;
            _keySignalBus = keySignalBus;
            _key = key;
            _callback = callback;
            _keySignalBus.Subscribe<TSignal>(key, Listener);
        }

        private void Listener(TSignal signal)
        {
            _keepWaiting = false;
            _keySignalBus.Unsubscribe<TSignal>(_key, Listener);
            _callback?.Invoke(signal);
        }
    }
}