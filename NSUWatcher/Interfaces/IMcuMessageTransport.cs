using System;

namespace NSUWatcher.Interfaces
{
    public interface IMcuMessageTransport : IDisposable
    {
        event EventHandler<TransportDataReceivedEventArgs> DataReceived;
        event EventHandler<TransportStateChangedEventArgs> StateChanged;

        bool IsConnected { get; }
        bool Start();
        void Stop();
        void Send(string command);
    }

    public class TransportDataReceivedEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public TransportDataReceivedEventArgs(string message)
        {
            Message = message;
        }
    }

    public enum TransportState
    {
        Disconnected,
        NotConnected,
        Connected,
        McuHalted
    }

    public class TransportStateChangedEventArgs : EventArgs
    {
        public TransportState State { get; private set; }

        public TransportStateChangedEventArgs(TransportState state)
        {
            State = state;
        }
    }
}
