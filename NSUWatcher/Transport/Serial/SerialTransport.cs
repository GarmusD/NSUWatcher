using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using NSUWatcher.Interfaces;
using Serilog;
using System.Diagnostics;
using System.IO;
using System;
using TransportDataContracts;
using System.Threading;
using NSUWatcher.Transport.Serial.Config;
using NSUWatcher.Exceptions;

namespace NSUWatcher.Transport.Serial
{
    public class SerialTransport : IMessageTransport
    {
        // Consts
        private const string ClientExeName = "nsuCom";

        // Properties
        public bool IsConnected => _serialProcess != null && !_serialProcess.HasExited;

        // Private
        private readonly ILogger _logger;
        private readonly JsonSerializerSettings _jsonSettings;
        private Process? _serialProcess = null;
        private readonly AutoResetEvent _dataReceivedEvent = new AutoResetEvent(false);
        private MessageData? _messageData = null;
        private bool _stopping = false;

        public SerialTransport(IConfiguration config, ILogger logger)
        {
            _logger = logger.ForContext<SerialTransport>();
            SerialConfig? cfg = config.GetSection("transport:serial").Get<SerialConfig?>();
            if (cfg is null)
            {
                _logger.Error("Configuration section \"transport:serial\" is missing.");
                throw new ConfigurationValueMissingException("Configuration section \"transport:serial\" is missing.");
            }

            if (!File.Exists(ClientExeName))
            {
                _logger.Error($"An required '{ClientExeName}' executable not found.");
                throw new FileNotFoundException(ClientExeName);
            }

            _jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            StartNsuComProcess(cfg);
        }

        private void StartNsuComProcess(SerialConfig cfg)
        {
            ProcessStartInfo psi = new ProcessStartInfo(ClientExeName, BuildArgs(cfg))
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            _serialProcess = Process.Start(psi);
            _serialProcess.BeginOutputReadLine();
            _serialProcess.OutputDataReceived += (s, e) =>
            {
                OnDataReceived(e.Data);
            };

            _serialProcess.BeginErrorReadLine();
            _serialProcess.ErrorDataReceived += (s, e) =>
            {
                _logger.Error($"SerialTransport received error output: {e.Data}");
            };

            _serialProcess.Exited += (s, e) =>
            {
                _serialProcess.Dispose();
                if (!_stopping)
                {
                    OutputMessage(new MessageData()
                    {
                        Destination = DataDestination.System,
                        Action = Constants.ActionComAppCrash
                    });
                }
            };
        }

        private string BuildArgs(SerialConfig cfg)
        {
            return $"{Constants.ArgComPort}{cfg.ComPort} " +
                    $"{Constants.ArgBaudRate}{cfg.BaudRate} " +
                    $"{Constants.ArgRebootBaudrate}{cfg.RebootMcu.BaudRate} " +
                    $"{Constants.ArgRebootDtrPulseOnly}{cfg.RebootMcu.DtrPulseOnly} " +
                    $"{Constants.ArgRebootDelay}{cfg.RebootMcu.Delay} " +
                    $"{Constants.ArgRebootReconnectDelay}{cfg.RebootMcu.ReconnectDelay}";
        }

        public IMessageData Receive()
        {
            _dataReceivedEvent.WaitOne();
            if (_messageData != null) return _messageData;
            // Return empty dataset
            return new MessageData();
        }

        public void Send(IMessageData command)
        {
            Send(new TransportData()
            {
                Destination = GetDestinationString(command.Destination),
                Action = command.Action,
                Content = command.Content
            });
        }

        private string GetDestinationString(DataDestination destination)
        {
            return destination switch 
            {
                DataDestination.System => Constants.DestinationSystem,
                DataDestination.Mcu => Constants.DestinationMcu,
                _ => throw new NotImplementedException($"Destination \"{destination}\" is not implemented.")
            };
        }

        public void Start()
        {
            Send(new TransportData()
            {
                Destination = Constants.DestinationSystem,
                Action = Constants.ActionConnect,
                Content = string.Empty
            });
        }

        public void Stop()
        {
            _stopping = true;
            Send(new TransportData()
            {
                Destination = Constants.DestinationSystem,
                Action = Constants.ActionDisconnect,
                Content = string.Empty
            });
        }

        private void OutputMessage(MessageData? value)
        {
            _messageData = value;
            _dataReceivedEvent.Set();
        }

        private void OnDataReceived(string dataStr)
        {
            TransportData? data = JsonConvert.DeserializeObject<TransportData?>(dataStr);

            if (data == null)
            {
                _logger.Error($"Error deserializing IpcData from string: {dataStr}");
                return;
            }

            if (data.Destination == Constants.DestinationLogger)
            {
                LogReceivedData(data);
                return;
            }

            MessageData message = new MessageData()
            {
                Destination = data.Destination switch
                {
                    Constants.DestinationMcu => DataDestination.Mcu,
                    Constants.DestinationSystem => DataDestination.System,
                    _ => DataDestination.Unknown
                }
            };
            if (message.Destination == DataDestination.Unknown)
            {
                _logger.Warning($"Destination '{data.Destination}' not implemented.");
                return;
            }
            message.Action = data.Action;
            message.Content = data.Content;
            OutputMessage(message);
        }

        private void LogReceivedData(ITransportData data)
        {
            switch (data.Action)
            {
                case Constants.ActionLogDebug:
                    _logger.Debug("nsuCom: " + data.Content);
                    return;
                case Constants.ActionLogInfo:
                    _logger.Information("nsuCom: " + data.Content);
                    return;
                case Constants.ActionLogWarning:
                    _logger.Warning("nsuCom: " + data.Content);
                    return;
                case Constants.ActionLogError:
                    _logger.Error("nsuCom: " + data.Content);
                    return;
                case Constants.ActionLogFatal:
                    _logger.Fatal("nsuCom: " + data.Content);
                    return;
                default:
                    _logger.Warning($"nsuCom: Unknown log action. Destination: {data.Destination}, Action: {data.Action}, Content: {data.Content}");
                    break;
            }
        }

        public void Send(ITransportData data)
        {
            if (!IsConnected) return;

            string dataStr = JsonConvert.SerializeObject(data, _jsonSettings);
            _serialProcess?.StandardInput.WriteLine(dataStr);
        }

        public void SendToMcu(string data)
        {
            Send(new TransportData()
            {
                Destination = Constants.DestinationMcu,
                Action = Constants.ActionData,
                Content = data
            });
        }

        public void Dispose()
        {
            Stop();
            Send(new TransportData()
            {
                Destination = DataDestination.System.ToString(),
                Action = Constants.ActionQuit,
                Content = string.Empty
            });
        }
    }
}
