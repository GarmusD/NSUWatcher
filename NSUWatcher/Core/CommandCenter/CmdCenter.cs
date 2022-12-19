using System;
using NSUWatcher.Interfaces.MCUCommands;
using Microsoft.Extensions.Hosting;
using NSUWatcher.Interfaces;
using System.Threading;
using Microsoft.Extensions.Configuration;
using NSU.Shared.Serializer;
using Timer = System.Timers.Timer;
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
        public bool Running => _msgTransport != null && _msgTransport.IsConnected;

        public IMcuMessageTransport MessageTransport { get => _msgTransport; set => SetMessageTransport(value); }

        public IMcuCommands MCUCommands => _mcuCommands;
        public IExternalCommands ExternalCommands => _externalCommands;

        // Private fields
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly MCUCommands _mcuCommands;
        private readonly ExternalCommands _externalCommands;
        private IMcuMessageTransport _msgTransport = null;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly Timer _guardTimer;

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

            _guardTimer = new Timer(TimeSpan.FromSeconds(15).TotalMilliseconds);

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
            string receivedLine = e.Message;

            if (receivedLine.StartsWith("JSON:", StringComparison.Ordinal))
            {
                ParseAndHandleReceivedMcuLine(receivedLine.Remove("JSON:"));
            }
            else if (receivedLine.StartsWith("GUARD", StringComparison.Ordinal))
            {
                ResetGuardTimer();
            }
            else if (receivedLine.StartsWith("DBG:", StringComparison.Ordinal))
            {
                _logger.LogDebug("MCU Debug: " + receivedLine.Remove("DBG:"));
            }
            else if (receivedLine.StartsWith("ERROR:", StringComparison.Ordinal))
            {
                _logger.LogError("MCU Error: " + receivedLine.Remove("ERROR:"));
            }
            else if (receivedLine.StartsWith("INFO:", StringComparison.Ordinal))
            {
                _logger.LogInformation("MCU Info: " + receivedLine.Remove("INFO:"));
            }
            else _logger.LogDebug("MCU Unknown data: " + receivedLine);
        }

        private void ParseAndHandleReceivedMcuLine(string receivedLine)
        {
            try
            {
                _logger.LogDebug($"ParseAndHandleReceivedMcuLine(): {receivedLine}");
                var cmd = _mcuCommands.FromMcu.Parse(receivedLine);
                if (cmd == null)
                {
                    _logger.LogWarning($"ParseAndHandleReceivedLine(): Unsupported command received: '{receivedLine}'.");
                    return;
                }
                OnMcuMessageReceived(cmd);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ParseAndHandleReceivedLine(): '{receivedLine}' Exception:");
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

        public void ResetGuardTimer()
        {
            _guardTimer.Enabled = false;
            _guardTimer.Enabled = true;
        }

        public void Dispose()
        {
            _msgTransport.Dispose();
            _guardTimer.Dispose();
        }
    }

    public static class StringExt
    {
        /// <summary>
        /// Extension method to remove a substring from an string start.
        /// Also trims resulting string.
        /// </summary>
        /// <param name="orig">A string to remove from</param>
        /// <param name="value">A substring to remove</param>
        /// <returns></returns>
        public static string Remove(this string orig, string value)
        {
            return orig.Remove(0, value.Length).Trim();
        }
    }
}

