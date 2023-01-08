using Microsoft.Extensions.Configuration;
using NSUWatcher.Interfaces;
using System;
using NSUWatcher.Transport.Serial.Config;
using NSUWatcher.Exceptions;
using System.IO.Ports;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Timer = System.Timers.Timer;

namespace NSUWatcher.Transport.Serial
{
    public class SerialTransport : IMcuMessageTransport, IHostedService
    {
        private enum MessageMarker
        {
            None,
            Guard,
            Json,
            Info,
            Debug,
            Error
        }

        // Consts
        private const int BufferSize = 1024;
        
        //Events
        public event EventHandler<TransportDataReceivedEventArgs> DataReceived;
        public event EventHandler<TransportStateChangedEventArgs> StateChanged;

        // Properties
        public TransportState TransportState => _transportState;
        public bool IsConnected => _serialPort != null && _serialPort.IsOpen;
        public char CommandDelimiter { get => _commandDelimiter; set => _commandDelimiter = value; }

        // Private
        private readonly ILogger _logger;
        private readonly SerialPort _serialPort;
        private readonly Timer _guardTimer;
        private char _commandDelimiter;
        private readonly byte[] _buffer;
        private int _bufferIdx;
        private TransportState _transportState = TransportState.NotConnected;
        private CancellationTokenSource _readPortCancel = null;
        

        public SerialTransport(IConfiguration config, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLoggerShort<SerialTransport>() ?? NullLoggerFactory.Instance.CreateLogger<SerialTransport>();
            _logger.LogDebug("Creating SerialTransport.");
            if(config == null)
            {
                _logger.LogError("SerialTransport: Config is null.");
                throw new ArgumentNullException("config");
            }

            SerialConfig cfg = LoadConfig(config);
            if(cfg == null)
            {
                string errMsg = "SerialConfig is null.";
                _logger.LogError(errMsg);
                throw new ConfigurationValueMissingException(errMsg);
            }

            if (!SerialPort.GetPortNames().Contains(cfg.ComPort))
            {
                string errMsg = $"ComPort '{cfg.ComPort}' does not exist.";
                _logger.LogCritical(errMsg);
                throw new ConfigurationValueMissingException(errMsg);
            }

            _logger.LogDebug($"Creating SerialPort({cfg.ComPort}, {cfg.BaudRate})");
            _serialPort = new SerialPort(cfg.ComPort, cfg.BaudRate)
            {
                RtsEnable = true
            };
            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.ErrorReceived += SerialPort_ErrorReceived;

            _guardTimer = new Timer(TimeSpan.FromSeconds(15).TotalMilliseconds);
            _guardTimer.Elapsed += GuardTimer_Elapsed;

            _buffer = new byte[BufferSize];
            _bufferIdx = 0;

            _commandDelimiter = '\n';
            _logger.LogTrace("SerialTransport created.");
        }

        private void GuardTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            OnStateChanged(TransportState.McuHalted);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("SerialTransport StartAsync()");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("SerialTransport StopAsync()");
            return Task.CompletedTask;
        }

        private SerialConfig LoadConfig(IConfiguration config)
        {
            SerialConfig cfg = config.GetSection("transport:serial").Get<SerialConfig>();
            if (cfg is null)
            {
                string errMsg = "Configuration section \"transport:serial\" is missing.";
                _logger.LogCritical(errMsg);
                throw new ConfigurationValueMissingException(errMsg);
            }

            if (string.IsNullOrEmpty(cfg.ComPort) || cfg.BaudRate <= 0)
            {
                string errMsg = "Configuration contains invalid ComPort or BaudRate values for section \"transport:serial\".";
                _logger.LogCritical(errMsg);
                throw new ConfigurationValueMissingException(errMsg);
            }

            return cfg;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _logger.LogDebug("SerialPort - DataReceived event.");
            var readCount = _serialPort.Read(_buffer, _bufferIdx, BufferSize - _bufferIdx);
            if (readCount > 0)
            {
                _bufferIdx += readCount;
                ProcessReceivedData();
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            _logger.LogError("SerialPort_ErrorReceived event.");
        }

        private Task ReadComPortAsync(CancellationToken token)
        {
            return Task.Run(() => 
            {
                _logger.LogDebug("SerialTransport: ReadComPortAsync()");
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        byte b = (byte)_serialPort.ReadByte();                        
                        _buffer[_bufferIdx++] = b;
                        if (b == _commandDelimiter && !token.IsCancellationRequested)
                        {
                            ProcessReceivedData();
                        }
                    }
                }
                catch (OperationCanceledException) { }
                _logger.LogDebug("SerialTransport: ReadComPortAsync() Done.");
            });
        }

        private void ProcessReceivedData()
        {
            while (true)
            {
                var delimiterIdx = Array.IndexOf(_buffer, (byte)_commandDelimiter, 0, _bufferIdx);
                if (delimiterIdx > -1)
                {
                    //CommandDelimiter is in buffer
                    string rcvStr = Encoding.ASCII.GetString(_buffer, 0, delimiterIdx);
                    if (_bufferIdx - 1 > delimiterIdx)
                    {
                        Array.Copy(_buffer, delimiterIdx + 1, _buffer, 0, _bufferIdx - delimiterIdx - 1);
                    }
                    _bufferIdx -= delimiterIdx + 1;

                    rcvStr = RemoveSeqNum(rcvStr);
                    MessageMarker marker;
                    (marker, rcvStr) = ExtractMessageMarker(rcvStr);
                    switch (marker)
                    {
                        case MessageMarker.None:
                            _logger.LogDebug("MCU Unknown data: " + rcvStr);
                            break;
                        case MessageMarker.Guard:
                            ResetGuardTimer();
                            break;
                        case MessageMarker.Json:
                            _logger.LogDebug($"MCU Json: {rcvStr}");
                            OnMessageReceived(rcvStr);
                            break;
                        case MessageMarker.Info:
                            _logger.LogInformation("MCU Info: " + rcvStr);
                            break;
                        case MessageMarker.Debug:
                            _logger.LogDebug("MCU Debug: " + rcvStr);
                            break;
                        case MessageMarker.Error:
                            _logger.LogError("MCU Error: " + rcvStr);
                            break;
                        default:
                            break;
                    }   
                }
                else 
                    return;
            }
        }

        private string RemoveSeqNum(string rcvStr)
        {
            if (rcvStr.StartsWith("0 "))
                return rcvStr.Substring("0 ".Length);
            return rcvStr;
        }

        private (MessageMarker, string) ExtractMessageMarker(string message)
        {
            MessageMarker marker = MessageMarker.None;
            string result;
            if (message.StartsWith("JSON:", StringComparison.Ordinal))
            {
                marker = MessageMarker.Json;
                result = message.Remove("JSON:");
            }
            else if (message.StartsWith("GUARD", StringComparison.Ordinal))
            {
                marker = MessageMarker.Guard;
                result = string.Empty;
            }
            else if (message.StartsWith("DBG:", StringComparison.Ordinal))
            {
                marker = MessageMarker.Debug;
                result = message.Remove("DBG:");
            }
            else if (message.StartsWith("ERROR:", StringComparison.Ordinal))
            {
                marker = MessageMarker.Error;
                result = message.Remove("ERROR:");
            }
            else if (message.StartsWith("INFO:", StringComparison.Ordinal))
            {
                marker = MessageMarker.Info;
                result = message.Remove("INFO:");
            }
            else
            {
                result = message;
            }
            return (marker, result);
        }
        
        private void OnMessageReceived(string message)
        {
            var evt = DataReceived;
            evt?.Invoke(this, new TransportDataReceivedEventArgs(message));
        }

        public void Send(string command)
        {
            if(_serialPort.IsOpen)
            {
                byte[] buffToWrite = Encoding.ASCII.GetBytes($"0 {command}{_commandDelimiter}");
                _serialPort.Write(buffToWrite, 0, buffToWrite.Length);
            }
        }

        public bool Start()
        {
            _logger.LogDebug("SerialTransport: Start()");
            bool result;
            try
            {
                _serialPort.Open();
                if(_serialPort.IsOpen)
                {
                    _readPortCancel = new CancellationTokenSource();
                    _ = ReadComPortAsync(_readPortCancel.Token);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError("SerialTransport: Start(): Exception: {ex}", ex);
            }
            finally
            {
                _logger.LogDebug($"SerialTransport: Start(): Finally: IsConnected: {IsConnected}");
                OnStateChanged(_serialPort.IsOpen ? TransportState.Connected : TransportState.NotConnected);
                result = IsConnected;
            }
            return result;
        }

        private void OnStateChanged(TransportState newState)
        {
            if (_transportState != newState)
            {
                _transportState = newState;
                var evt = StateChanged;
                evt?.Invoke(this, new TransportStateChangedEventArgs(newState));
            }
        }

        /*
        private async Task SendDTRPulseNoConnect(int delayMillis)
        {
            _logger.Debug($"Sending DTR signal...");
            _transport.SerialPort.DtrEnable = true;
            await Task.Delay(delayMillis);
            _transport.SerialPort.DtrEnable = false;
            await Task.Delay(delayMillis);
        }

        private async Task<bool> SendDtrPulseWithConnect(int baudRate, int delayMillis)
        {
            _logger.Debug("Connecting with DTR enabled...");
            _transport.CurrentSerialSettings.DtrEnable = true;
            _transport.CurrentSerialSettings.BaudRate = baudRate;
            if (_messenger.StartListening())
            {
                await Task.Delay(delayMillis);
                _messenger.StopListening();
                _transport.CurrentSerialSettings.BaudRate = _baudRate;
                _transport.CurrentSerialSettings.DtrEnable = false;
                await Task.Delay(delayMillis);
                _logger.Debug("Done. DTR is disabled.");
                return true;
            }
            _logger.Error("Messenger not started listening with DTR enabled.");
            return false;
        }

        public async Task RebootMcu(bool DtrPulseOnly, int baudRate, int delayMillis)
        {
            _logger.Debug("RebootMcu() called.");
            Disconnect();
            if (DtrPulseOnly)
                await SendDTRPulseNoConnect(delayMillis);
            else
                await SendDtrPulseWithConnect(baudRate, delayMillis);
            Connect();
        }
        */

        public void Stop()
        {
            _logger.LogDebug("SerialTransport: Stop()");
            _readPortCancel?.Cancel();
            _serialPort.Close();
        }

        public void ResetGuardTimer()
        {
            _guardTimer.Enabled = false;
            _guardTimer.Enabled = true;
        }

        public void Dispose()
        {
            _serialPort?.Dispose();
            _readPortCancel?.Dispose();
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
