namespace AleVerDes.Signals
{
    /// <summary>
    /// Delegate type for signal handlers that receive signal data.
    /// </summary>
    /// <typeparam name="TSignal">The type of signal data that will be passed to the handler.</typeparam>
    /// <param name="signal">The signal data instance.</param>
    public delegate void SignalReceivedDelegate<in TSignal>(TSignal signal);
}