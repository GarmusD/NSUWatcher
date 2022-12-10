using System;
using Serilog;
using NSUWatcher.Interfaces.MCUCommands;
using TransportDataContracts;
using Microsoft.Extensions.Hosting;
using NSUWatcher.Interfaces;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Configuration;
using NSUWatcher.Transport.Serial;
using NSU.Shared.Serializer;

namespace NSUWatcher.CommandCenter
{
    public class CmdCenter : ICmdCenter, IHostedService
    {
        public event EventHandler<McuMessageReceivedEventArgs>? McuMessageReceived;
        public event EventHandler<SystemMessageEventArgs>? SystemMessageReceived;
        public event EventHandler<ExternalCommandEventArgs>? ExternalCommandReceived;
        public event EventHandler<ManualCommandEventArgs>? ManualCommandReceived;

        // Properties
        public bool Running => _msgTransport != null && _msgTransport.IsConnected;

        public IMessageTransport MessageTransport { get => _msgTransport; set => _msgTransport =value; }
        public IMcuCommands MCUCommands => _mcuCommands;
        public IExternalCommands ExternalCommands => _externalCommands;

        // Private fields
        private readonly ILogger _logger;
        private readonly MCUCommands _mcuCommands;
        private readonly ExternalCommands _externalCommands;
        private IMessageTransport _msgTransport;
        private readonly IHostApplicationLifetime _lifetime;
        private CancellationTokenSource? _cts = null;

        public CmdCenter(IConfiguration config, IHostApplicationLifetime lifetime, ILogger logger)
        {
            _logger = logger.ForContext<CmdCenter>() ?? throw new ArgumentNullException(nameof(logger), "Instance of ILogger cannot be null.");
            _lifetime = lifetime;

            _logger.Debug("Creating CmdCenter...");

            _logger.Debug("Creating default IMessageTransport - SerialTransport...");
            _msgTransport = new SerialTransport(config, logger);

            _logger.Debug("Creating MCUCommands.");
            _mcuCommands = new MCUCommands(DefaultSendToMcuAction, logger);

            _logger.Debug("Creating ExternalCommands.");
            _externalCommands = new ExternalCommands( new NsuSerializer() );

            _logger.Debug("CmdCenter created.");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = new CancellationTokenSource();
            _ = ReadTransportDataAsync(_cts.Token);
            _msgTransport.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _msgTransport.Stop();
            _cts?.Cancel();
            return Task.CompletedTask;
        }

        private Task ReadTransportDataAsync(CancellationToken token)
        {
            return Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        // This is a blocking call
                        IMessageData data = _msgTransport.Receive();
                        switch (data.Destination)
                        {
                            case DataDestination.Mcu:
                                McuDataReceivedHandler(data);
                                break;
                            case DataDestination.System:
                                SystemDataReceivedHandler(data);
                                break;
                            default:
                                _logger.Warning($"ReadTransportDataAsync(): Destination \"{data.Destination}\" is not implemented.");
                                break;
                        }
                    }
                }
                catch (OperationCanceledException) { }
            });
        }

        private void McuDataReceivedHandler(IMessageData message)
        {
            string receivedLine = message.Content;

            if (receivedLine.StartsWith("JSON:", StringComparison.Ordinal))
            {
                ParseAndHandleReceivedMcuLine(receivedLine.Remove("JSON:"));
            }
            else if (receivedLine.StartsWith("DBG:", StringComparison.Ordinal))
            {
                _logger.Debug("MCU Debug: " + receivedLine.Remove("DBG:"));
            }
            else if (receivedLine.StartsWith("ERR:", StringComparison.Ordinal))
            {
                _logger.Error("MCU Error: " + receivedLine.Remove("ERR:"));
            }
            else if (receivedLine.StartsWith("INFO:", StringComparison.Ordinal))
            {
                _logger.Debug("MCU Info: " + receivedLine.Remove("INFO:"));
            }
            else _logger.Debug("MCU Unknown data: " + receivedLine);
        }

        private void ParseAndHandleReceivedMcuLine(string receivedLine)
        {
            try
            {
                var cmd = _mcuCommands.FromMcu.Parse(receivedLine);
                if (cmd == null)
                {
                    _logger.Warning($"ParseAndHandleReceivedLine(): Unsupported command received: '{receivedLine}'.");
                    return;
                }
                OnMcuMessageReceived(cmd);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"ParseAndHandleReceivedLine(): '{receivedLine}' Exception:");
            }
        }

        private void SystemDataReceivedHandler(IMessageData message)
        {
            SysMessage sysMessage = message.Action switch
            {
                Constants.ActionConnected => SysMessage.TransportConnected,
                Constants.ActionConnectFailed => SysMessage.TransportConnectFailed,
                Constants.ActionDisconnected => SysMessage.TransportDisconnected,
                Constants.ActionMcuHalted => SysMessage.McuCrashed,
                Constants.ActionComAppCrash => SysMessage.TransportAppCrashed,
                _ => throw new NotImplementedException($"SystemDataReceivedHandler(): Action \"{message.Action}\" is not implemented.")
            };

            var evt = SystemMessageReceived;
            evt?.Invoke(this, new SystemMessageEventArgs(sysMessage));

            if (sysMessage == SysMessage.TransportAppCrashed) 
                _lifetime.StopApplication();
        }

        private void OnMcuMessageReceived(IMessageFromMcu data)
        {
            var evt = McuMessageReceived;
            evt?.Invoke(this, new McuMessageReceivedEventArgs(data));
        }

        /*private void OnExternalCommandReceived(IExternalCommand command, INsuUser nsuUser, Action<IExternalCommandResult, object> onCommandResult, object context)
        {
            var evt = ExternalCommandReceived;
            evt?.Invoke(this, new ExternalCommandEventArgs(command, nsuUser, onCommandResult, context));
        }*/

        private void OnManualCommandReceived(string command)
        {
            var evt = ManualCommandReceived;
            evt?.Invoke(this, new ManualCommandEventArgs(command));
        }


        public void SendToMCU(Func<IToMcuCommands, ICommandToMCU> getCmd)
        {
            ICommandToMCU cmdToMcu = getCmd(_mcuCommands.ToMcu);
            SendToArduino(cmdToMcu.Value);
        }

        private void DefaultSendToMcuAction(string command)
        {
            SendToArduino(command);
        }

        private void SendToArduino(string cmd)
        {
            _logger.Debug($"SendToArduino(): cmd: '{cmd}'");
            _msgTransport.Send(new MessageData() 
            {
                Destination = DataDestination.Mcu,
                Action = string.Empty,
                Content = cmd,
            });
        }

        /*public void ExecExternalCommand(IExternalCommand command, INsuUser nsuUser, Action<IExternalCommandResult, object> onCommandResult, object context)
        {
            OnExternalCommandReceived(command, nsuUser, onCommandResult, context);
        }*/

        public IExternalCommandResult? ExecExternalCommand(IExternalCommand command, INsuUser nsuUser, object? context = null)
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

