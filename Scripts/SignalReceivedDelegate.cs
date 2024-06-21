namespace TaigaGames.Signals
{
    public delegate void SignalReceivedDelegate<in TSignal>(TSignal signal);
}