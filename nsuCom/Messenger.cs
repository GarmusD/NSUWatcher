using CommandMessenger.TransportLayer;
using CommandMessenger;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace serialTransport
{
    internal class Messenger : IDisposable
    {
        public event EventHandler<EventArgs> ArduinoCrashed;
        public event EventHandler<McuDataEventArgs> McuDataReceived;
        public bool IsConnected => _connected;

        private readonly ILogger _logger;
        private readonly CmdMessenger _messenger;
        private readonly SerialTransport _transport;
        readonly Timer _guardTimer;
        private bool _connected = false;
        private readonly int _baudRate;

        public Messenger(string port, int baudRate, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.Debug("Messenger() creating...");
            _baudRate = baudRate;

            _transport = new SerialTransport
            {
                CurrentSerialSettings = { PortName = port, BaudRate = baudRate, DtrEnable = false, RtsEnable = false }
            };

            _messenger = new CmdMessenger(_transport, ' ', '\n')
            {
                BoardType = BoardType.Bit32, // Set if it is communicating with a 16- or 32-bit Arduino board
                PrintLfCr = false
            };
            _messenger.Attach(Messenger_OnReceivedCommand);

            _guardTimer = new Timer
            {
                Interval = TimeSpan.FromMinutes(2).TotalMilliseconds
            };
            _guardTimer.Elapsed += OnGuardTimer;
            // Do not start _guardTimer.
            // It will be started by getting the first 'GUARD' message
            _logger.Debug("Messenger() created.");
        }

        private void OnArduinoCrashed()
        {
            var evt = ArduinoCrashed;
            evt?.Invoke(this, EventArgs.Empty);
        }

        void OnGuardTimer(object sender, ElapsedEventArgs e)
        {
            _logger.Debug($"OnGuardTimer(). ARDUINO CRASHED - raising OnArduinoCrashed!");
            try
            {
                _guardTimer.Enabled = false;
                OnArduinoCrashed();
            }
            catch (Exception ex)
            {
                _logger.Error($"OnGuardTimer(): OnArduinoCrashed handler exception: {ex}");
            }
            return;
        }

        public bool Connect()
        {
            _logger.Debug($"Start()");

            if (_messenger.StartListening())
            {
                _logger.Info($"CmdMessenger started. OK.");
                _connected = true;
                return true;
            }
            else
            {
                _logger.Error($"Start() - Messenger is Not Listening");
                _connected = false;
                return false;
            }
        }

        public void Disconnect()
        {
            if (_connected)
            {
                _guardTimer.Stop();
                _messenger.StopListening();
            }
        }

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

        private void Messenger_OnReceivedCommand(ReceivedCommand arguments)
        {
            string receivedLine = arguments.CommandString();
            receivedLine = receivedLine.Replace('\n'.ToString(), "").Replace('\r'.ToString(), "");

            if (receivedLine.Length > 2)
            {
                // Remove cmdID and space
                receivedLine = receivedLine.Remove(0, receivedLine.IndexOf(' ') + 1);

                // Skip VisualMicro debug messages
                if (receivedLine.StartsWith("VMD", StringComparison.Ordinal))
                    return;

                // MCU "heartbeat"
                if (receivedLine.Equals("GUARD"))
                {
                    ResetGuardTimer();
                    return;
                }
            }
            else
                receivedLine = $"ERR: From the MCU received invalid data line: '{receivedLine}'.";

            var evt = McuDataReceived;
            evt?.Invoke(this, new McuDataEventArgs(receivedLine));
        }

        public void SendToArduino(string command, bool waitAck = false)
        {
            var scmd = new SendCommand(0, command)
            {
                ReqAc = waitAck
            };
            _messenger.SendCommand(scmd);
        }

        public void ResetGuardTimer()
        {
            _guardTimer.Enabled = false;
            _guardTimer.Enabled = true;
        }

        public void Dispose()
        {
            Disconnect();
            _transport?.Dispose();
            _messenger?.Dispose();
            _guardTimer?.Dispose();
        }
    }

    public class McuDataEventArgs
    {
        public string DataString { get; }

        public McuDataEventArgs(string dataString)
        {
            DataString = dataString;
        }
    }
}
