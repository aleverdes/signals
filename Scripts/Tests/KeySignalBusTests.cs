using System;
using NUnit.Framework;
using AleVerDes.Signals;

namespace AleVerDes.Signals.Tests
{
    /// <summary>
    /// Unit tests for the KeySignalBus class.
    /// </summary>
    public class KeySignalBusTests
    {
        private KeySignalBus<string> _keySignalBus;
        private TestSignalHandler _handler;

        [SetUp]
        public void Setup()
        {
            _keySignalBus = new KeySignalBus<string>();
            _handler = new TestSignalHandler();
        }

        [TearDown]
        public void TearDown()
        {
            _handler.Reset();
        }

        [Test]
        public void Subscribe_ValidKeyAndReceiver_AddsToSubscribers()
        {
            _keySignalBus.Subscribe("key1", _handler.Handle);

            Assert.AreEqual(1, _keySignalBus.GetSubscriberCount<TestSignal>("key1"));
            Assert.IsTrue(_keySignalBus.HasSubscribers<TestSignal>("key1"));
        }

        [Test]
        public void Subscribe_NullReceiver_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _keySignalBus.Subscribe("key1", (SignalReceivedDelegate<TestSignal>)null));
        }

        [Test]
        public void Invoke_SignalWithCorrectKey_CallsHandler()
        {
            _keySignalBus.Subscribe("key1", _handler.Handle);
            var signal = new TestSignal { Value = 42 };

            _keySignalBus.Invoke("key1", signal);

            Assert.IsTrue(_handler.WasCalled);
            Assert.AreEqual(signal.Value, _handler.ReceivedValue);
        }

        [Test]
        public void Invoke_SignalWithWrongKey_DoesNotCallHandler()
        {
            _keySignalBus.Subscribe("key1", _handler.Handle);
            var signal = new TestSignal { Value = 42 };

            _keySignalBus.Invoke("key2", signal);

            Assert.IsFalse(_handler.WasCalled);
        }

        [Test]
        public void GetActiveKeys_ReturnsKeysWithSubscriptions()
        {
            _keySignalBus.Subscribe<TestSignal>("key1", _handler.Handle);
            _keySignalBus.Subscribe<OtherTestSignal>("key2", _handler.HandleOther);

            var activeKeys = _keySignalBus.GetActiveKeys();

            Assert.Contains("key1", activeKeys);
            Assert.Contains("key2", activeKeys);
        }

        [Test]
        public void Clear_SpecificKey_RemovesAllSubscriptionsForThatKey()
        {
            _keySignalBus.Subscribe<TestSignal>("key1", _handler.Handle);
            _keySignalBus.Subscribe<OtherTestSignal>("key1", _handler.HandleOther);
            _keySignalBus.Subscribe<TestSignal>("key2", _handler.Handle);

            _keySignalBus.Clear("key1");

            Assert.AreEqual(0, _keySignalBus.GetSubscriberCount<TestSignal>("key1"));
            Assert.AreEqual(0, _keySignalBus.GetSubscriberCount<OtherTestSignal>("key1"));
            Assert.AreEqual(1, _keySignalBus.GetSubscriberCount<TestSignal>("key2"));
        }

        [Test]
        public void Clear_SpecificSignalTypeAndKey_RemovesOnlyThatSignalTypeForKey()
        {
            _keySignalBus.Subscribe<TestSignal>("key1", _handler.Handle);
            _keySignalBus.Subscribe<OtherTestSignal>("key1", _handler.HandleOther);

            _keySignalBus.Clear<TestSignal>("key1");

            Assert.AreEqual(0, _keySignalBus.GetSubscriberCount<TestSignal>("key1"));
            Assert.AreEqual(1, _keySignalBus.GetSubscriberCount<OtherTestSignal>("key1"));
        }

        [Test]
        public void ClearAll_RemovesAllSubscriptions()
        {
            _keySignalBus.Subscribe<TestSignal>("key1", _handler.Handle);
            _keySignalBus.Subscribe<OtherTestSignal>("key2", _handler.HandleOther);

            _keySignalBus.ClearAll();

            Assert.AreEqual(0, _keySignalBus.GetSubscriberCount<TestSignal>("key1"));
            Assert.AreEqual(0, _keySignalBus.GetSubscriberCount<OtherTestSignal>("key2"));
        }

        // Test signal types
        private struct TestSignal
        {
            public int Value;
        }

        private struct OtherTestSignal
        {
            public string Message;
        }

        // Test handler
        private class TestSignalHandler
        {
            public bool WasCalled { get; private set; }
            public int ReceivedValue { get; private set; }
            public bool OtherWasCalled { get; private set; }

            public void Handle(TestSignal signal)
            {
                WasCalled = true;
                ReceivedValue = signal.Value;
            }

            public void HandleOther(OtherTestSignal signal)
            {
                OtherWasCalled = true;
            }

            public void Reset()
            {
                WasCalled = false;
                ReceivedValue = 0;
                OtherWasCalled = false;
            }
        }
    }
}
