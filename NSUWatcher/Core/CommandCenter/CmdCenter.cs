using System;
using NSUWatcher.Interfaces.MCUCommands;
using Microsoft.Extensions.Hosting;
using NSUWatcher.Interfaces;
using System.Threading;
using Microsoft.Extensions.Configuration;
using NSU.Shared.Serializer;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSUWatcher.Interfaces.NsuUsers;

namespace NSUWatcher.CommandCenter
{
    public class CmdCenter : ICmdCenter, IHostedService
    {
        public event EventHandler<McuMessageReceivedEventArgs> McuMessageReceived;
        public event EventHandler<SystemMessageEventArgs> SystemMessageReceived;
        public event EventHandler<ExternalCommandEventArgs> ExternalCommandReceived;
        public event EventHandler<ManualCommandEventArgs> ManualCommandReceived;

        // Properties
        public IMcuMessageTransport MessageTransport { get => _msgTransport; set => SetMessageTransport(value); }
        public IMcuCommands MCUCommands => _mcuCommands;
        public IExternalCommands ExtCommandFactory => _externalCommands;

        // Private fields
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly MCUCommands _mcuCommands;
        private readonly ExternalCommands _externalCommands;
        private IMcuMessageTransport _msgTransport = null;
        private readonly IHostApplicationLifetime _lifetime;
        

        public CmdCenter(IMcuMessageTransport transport, IConfiguration config, IHostApplicationLifetime lifetime, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory?.CreateLogger(nameof(CmdCenter)) ?? NullLoggerFactory.Instance.CreateLogger(nameof(CmdCenter));
            _logger.LogDebug("Creating ...");
            _config = config;
            _lifetime = lifetime;

            MessageTransport = transport;

            _logger.LogDebug("Creating MCUCommands.");
            _mcuCommands = new MCUCommands(DefaultSendToMcuAction, loggerFactory);

            _logger.LogDebug("Creating ExternalCommands.");
            _externalCommands = new ExternalCommands( new NsuSerializer() );

            _logger.LogTrace("Created.");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("StartAsync(). Do nothing.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("StopAsync(). Do nothing.");
            return Task.CompletedTask;
        }

        private void SetMessageTransport(IMcuMessageTransport value)
        {
            if (_msgTransport != null)
            {
                _msgTransport.DataReceived -= MessageTransport_DataReceived;
                _msgTransport.StateChanged -= MessageTransport_StateChanged;
                _msgTransport.Dispose();
            }
            _msgTransport = value ?? throw new ArgumentNullException("MessageTransport value cannot be null.");
            _msgTransport.DataReceived += MessageTransport_DataReceived;
            _msgTransport.StateChanged += MessageTransport_StateChanged;
        }
        
        private void MessageTransport_DataReceived(object sender, TransportDataReceivedEventArgs e)
        {
            try
            {
                var receivedMessage = e.Message;
                var cmd = _mcuCommands.FromMcu.Parse(receivedMessage);
                if (cmd == null)
                {
                    _logger.LogWarning($"ParseAndHandleReceivedLine(): Unsupported command received: '{receivedMessage}'.");
                    return;
                }
                OnMcuMessageReceived(cmd);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ParseAndHandleReceivedLine(): Exception: {ex}");
            }
        }


        private void OnMcuMessageReceived(IMessageFromMcu data)
        {
            var evt = McuMessageReceived;
            evt?.Invoke(this, new McuMessageReceivedEventArgs(data));
        }
        
        private void MessageTransport_StateChanged(object sender, TransportStateChangedEventArgs e)
        {
            _logger.LogDebug($"MessageTransport_StateChanged: {e.State}");
            var sysMessage = e.State switch
            {
                TransportState.Connected => SysMessage.TransportConnected,
                TransportState.NotConnected => SysMessage.TransportConnectFailed,
                TransportState.Disconnected => SysMessage.TransportDisconnected,
                TransportState.McuHalted => SysMessage.McuCrashed,
                _ => throw new NotImplementedException($"SystemDataReceivedHandler(): Action \"{e.State}\" is not implemented."),
            };
            var evt = SystemMessageReceived;
            evt?.Invoke(this, new SystemMessageEventArgs(sysMessage));
        }

        private void OnManualCommandReceived(string command)
        {
            var evt = ManualCommandReceived;
            evt?.Invoke(this, new ManualCommandEventArgs(command));
        }


        public void SendToMCU(Func<IToMcuCommands, ICommandToMCU> getCmd)
        {
            ICommandToMCU cmdToMcu = getCmd(_mcuCommands.ToMcu);
            SendToMcu(cmdToMcu.Value);
        }

        private void DefaultSendToMcuAction(string command)
        {
            SendToMcu(command);
        }

        private void SendToMcu(string cmd)
        {
            _logger.LogDebug($"SendToArduino(): cmd: '{cmd}'");
            _msgTransport?.Send(cmd);
        }

        public IExternalCommandResult ExecExternalCommand(IExternalCommand command, INsuUser nsuUser, object context = null)
        {
            ExternalCommandEventArgs args = new ExternalCommandEventArgs(command, nsuUser, context);
            ExternalCommandReceived?.Invoke(this, args);
            return args.Result;
        }

        public void ExecManualCommand(string command)
        {
            OnManualCommandReceived(command);
        }

        public void Dispose()
        {
            _msgTransport.Dispose();
        }
    }
}

