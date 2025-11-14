using System;
using NUnit.Framework;
using AleVerDes.Signals;

namespace AleVerDes.Signals.Tests
{
    /// <summary>
    /// Unit tests for the SignalBus class.
    /// </summary>
    public class SignalBusTests
    {
        private SignalBus _signalBus;
        private TestSignalHandler _handler;

        [SetUp]
        public void Setup()
        {
            _signalBus = new SignalBus();
            _handler = new TestSignalHandler();
        }

        [TearDown]
        public void TearDown()
        {
            _handler.Reset();
        }

        [Test]
        public void Subscribe_ValidReceiver_AddsToSubscribers()
        {
            _signalBus.Subscribe<TestSignal>(_handler.Handle);

            Assert.AreEqual(1, _signalBus.GetSubscriberCount<TestSignal>());
            Assert.IsTrue(_signalBus.HasSubscribers<TestSignal>());
        }

        [Test]
        public void Subscribe_NullReceiver_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _signalBus.Subscribe<TestSignal>(null));
        }

        [Test]
        public void Unsubscribe_ValidReceiver_RemovesFromSubscribers()
        {
            _signalBus.Subscribe<TestSignal>(_handler.Handle);
            _signalBus.Unsubscribe<TestSignal>(_handler.Handle);

            // Force cleanup
            _signalBus.Invoke(new TestSignal());

            Assert.AreEqual(0, _signalBus.GetSubscriberCount<TestSignal>());
            Assert.IsFalse(_signalBus.HasSubscribers<TestSignal>());
        }

        [Test]
        public void Unsubscribe_NullReceiver_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _signalBus.Unsubscribe<TestSignal>(null));
        }

        [Test]
        public void Invoke_SignalWithSubscribers_CallsHandler()
        {
            _signalBus.Subscribe<TestSignal>(_handler.Handle);
            var signal = new TestSignal { Value = 42 };

            _signalBus.Invoke(signal);

            Assert.IsTrue(_handler.WasCalled);
            Assert.AreEqual(signal.Value, _handler.ReceivedValue);
        }

        [Test]
        public void Invoke_SignalWithoutSubscribers_DoesNotThrow()
        {
            var signal = new TestSignal { Value = 42 };

            Assert.DoesNotThrow(() => _signalBus.Invoke(signal));
        }

        [Test]
        public void Invoke_HandlerThrowsException_ContinuesToOtherHandlers()
        {
            var handler1 = new TestSignalHandler();
            var handler2 = new TestSignalHandler();

            _signalBus.Subscribe<TestSignal>(_ => throw new Exception("Test exception"));
            _signalBus.Subscribe<TestSignal>(handler2.Handle);

            var signal = new TestSignal { Value = 42 };

            Assert.DoesNotThrow(() => _signalBus.Invoke(signal));
            Assert.IsTrue(handler2.WasCalled);
        }

        [Test]
        public void Clear_SpecificSignalType_RemovesAllSubscribersForThatType()
        {
            _signalBus.Subscribe<TestSignal>(_handler.Handle);
            _signalBus.Subscribe<OtherTestSignal>(_handler.HandleOther);

            _signalBus.Clear<TestSignal>();

            Assert.AreEqual(0, _signalBus.GetSubscriberCount<TestSignal>());
            Assert.AreEqual(1, _signalBus.GetSubscriberCount<OtherTestSignal>());
        }

        [Test]
        public void ClearAll_RemovesAllSubscribers()
        {
            _signalBus.Subscribe<TestSignal>(_handler.Handle);
            _signalBus.Subscribe<OtherTestSignal>(_handler.HandleOther);

            _signalBus.ClearAll();

            Assert.AreEqual(0, _signalBus.GetSubscriberCount<TestSignal>());
            Assert.AreEqual(0, _signalBus.GetSubscriberCount<OtherTestSignal>());
        }

        [Test]
        public void GetSubscriberCount_NoSubscribers_ReturnsZero()
        {
            Assert.AreEqual(0, _signalBus.GetSubscriberCount<TestSignal>());
        }

        [Test]
        public void HasSubscribers_NoSubscribers_ReturnsFalse()
        {
            Assert.IsFalse(_signalBus.HasSubscribers<TestSignal>());
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
