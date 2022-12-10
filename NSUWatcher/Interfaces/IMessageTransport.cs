using System;

namespace NSUWatcher.Interfaces
{
    public interface IMessageTransport : IDisposable
    {
        bool IsConnected { get; }
        void Start();
        void Stop();
        void Send(IMessageData command);
        IMessageData Receive();
    }
}
