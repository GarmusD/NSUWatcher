using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSUWatcher.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NSUWatcher.Transport.TestTransport
{
    public class TestTransport : IMcuMessageTransport, IHostedService
    {
        public bool IsConnected => _isConnected;

        public event EventHandler<TransportDataReceivedEventArgs> DataReceived;
        public event EventHandler<TransportStateChangedEventArgs> StateChanged;

        private bool _isConnected = false;
        private readonly CommandSender _sender;
        private readonly TestCommands _commands;
        private CancellationTokenSource _senderCancellation = null;
        private CancellationTokenSource _tasksCancellation = null;

        public TestTransport(ILoggerFactory loggerFactory)
        {
            _sender = new CommandSender(CommandReceived);
            _commands = new TestCommands(_sender, loggerFactory);
        }

        public void Dispose()
        {
            
        }

        public void Send(string command)
        {
            _commands.Execute(command);
        }

        public bool Start()
        {
            _isConnected = true;
            _senderCancellation = new CancellationTokenSource();
            _ = _sender.RunAsync(_senderCancellation.Token);
            _tasksCancellation = new CancellationTokenSource();
            _ = _commands.RunAsync(_tasksCancellation.Token);
            OnStateChanged(TransportState.Connected);
            return true;
        }

        public void Stop()
        {
            _tasksCancellation?.Cancel();
            _senderCancellation?.Cancel();
            _isConnected = false;
        }

        private void CommandReceived(string command)
        {
            DataReceived?.Invoke(this, new TransportDataReceivedEventArgs(command));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void OnStateChanged(TransportState newState)
        {
            var evt = StateChanged;
            evt?.Invoke(this, new TransportStateChangedEventArgs(newState));
        }
    }
}
