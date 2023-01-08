using NSU.Shared.NSUNet;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;
using NSU.Shared.DTO.NsuNet;
using System.Collections.Generic;
using System.Threading;
using Timer = System.Timers.Timer;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace NSUWatcher.Services.NSUWatcherNet
{
    public class NetClientDataReceivedEventArgs
    {
        public NetClientData ClientData { get; }
        public InternalArgs Args { get; }

        public NetClientDataReceivedEventArgs(NetClientData clientData, InternalArgs args)
        {
            ClientData = clientData;
            Args = args;
        }
    }

    class NetClient
    {
        public const int PING_INTERVAL = 15 * 60;

        //public event EventHandler<NetClientDataReceivedEventArgs>? DataReceived;
        public event EventHandler<EventArgs> Disconnected;

        public bool Connected => !_disconnected && _tcpClient != null && _tcpClient.Connected;
        public NetClientData ClientData => _clientData;

        private readonly ILogger _logger;
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _netStream;
        private readonly InternalArgBuilder _idra = new InternalArgBuilder();
        private readonly NetClientData _clientData;
        private readonly NetMessenger.Messenger _netMessenger;
        private readonly NetProtocol _protocol = new NetProtocol();
        private readonly Queue<byte[]> _sendQueue = new Queue<byte[]>();
        private readonly object _lockObj = new object();
        private readonly AutoResetEvent _are = new AutoResetEvent(false);
        private readonly byte[] _rcvBuffer;
        //response timer
        private readonly Timer _respTimer;
        private readonly Timer _pingTimer;

        private bool _disconnected = false;

        public NetClient(TcpClient client, NetMessenger.Messenger netMessenger, ILoggerFactory loggerFactory)
        {
            _tcpClient = client;
            _logger = loggerFactory?.CreateLoggerShort<NetClient>() ?? NullLoggerFactory.Instance.CreateLogger<NetClient>();
            _netMessenger = netMessenger;
            _netStream = client.GetStream();
            _rcvBuffer = new byte[client.ReceiveBufferSize];
            _clientData = new NetClientData
            {
                ProtocolVersion = 2,
                ClientID = Guid.NewGuid(),
                ClientType = NetClientData.NetClientType.Unknow
            };

            _respTimer = new Timer
            {
                Interval = TimeSpan.FromSeconds(15).TotalMilliseconds //15sek to respond
            };
            _respTimer.Elapsed += RespTimer_Elapsed;
            _respTimer.Start();

            _pingTimer = new Timer
            {
                Interval = TimeSpan.FromMinutes(5).TotalMilliseconds
            };
            _pingTimer.Elapsed += PingTimer_Elapsed;
        }

        private void PingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _logger.LogDebug("PingTimer_Elapsed()");
            Send(new NetMessage(new PingRequest()));
        }

        private void RespTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //No response over time. Old client?
            _logger.LogDebug("RespTimer_Elapsed()");
            DisconnectAndRaise();
        }

        private void ResetPinger()
        {
            _pingTimer.Enabled = false;
            _pingTimer.Enabled = true;
        }

        /*protected void OnDataReceived(NetClientData ClientData, InternalArgs args)
        {
            DataReceived?.Invoke(this, new NetClientDataReceivedEventArgs(ClientData, args));
        }*/

        public Task StartListeningAsync()
        {
            _logger.LogDebug("StartListeningAsync()");
            _ = RunQueueTaskAsync();
            return Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        var count = _netStream.Read(_rcvBuffer, 0, _rcvBuffer.Length);
                        if (count > 0)
                        {
                            ResetPinger();
                            _respTimer.Enabled = false;
                            ProcessReceivedData(count);
                        }
                        else
                        {
                            _logger.LogDebug("Received zero data. StartListeningAsync() terminating.");
                            break;
                        }
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is ObjectDisposedException)
                {
                    DisconnectAndRaise();
                }
            });
        }

        private void ProcessReceivedData(int count)
        {
            _idra.Process(_rcvBuffer, 0, count);
            while (_idra.DataAvailable)
            {
                NetMessage message = null;
                var args = _idra.GetArgs();
                if (args.DataType == NetDataType.String)
                {
                    message = new NetMessage((string)args.Data);
                }                
                if (message != null)
                {
                    _logger.LogDebug($"Received data: {message.AsString()}");
                    INetMessage response = _netMessenger.ProcessNetMessage(message, _clientData);
                    if (response != null)
                        Send(response);
                }
                else
                    _logger.LogDebug($"ProcessReceivedData(count:{count}) - message is null.");
            }
        }

        public void Send(INetMessage message)
        {
            _logger.LogDebug($"Sending to user: {message.AsString()}");
            var list = _protocol.Encode(message, ClientData.ProtocolVersion);
            lock(_lockObj)
            {
                foreach (var data in list)
                {
                    _logger.LogDebug("Send() - message enqueued.");
                    _sendQueue.Enqueue(data);
                }
            }
            //Signal sendQueue about new data
            _are.Set();
        }

        private Task RunQueueTaskAsync()
        {
            return Task.Run(() =>
            {
                _logger.LogDebug("RunQueueTaskAsync()");
                byte[] data;
                while (!_disconnected)
                {
                    while (_sendQueue.Any())
                    {
                        lock (_lockObj)
                        {
                            data = _sendQueue.Dequeue();
                        }
                        _logger.LogDebug("RunQueueTaskAsync() - data dequeued.");
                        if (!SendBuffer(data))
                        {
                            DisconnectAndRaise();
                            return;
                        }
                        _logger.LogDebug("RunQueueTaskAsync() - data is sent.");
                    }
                    _logger.LogDebug("Waiting for next data in the sendQueue.");
                    _are.WaitOne();
                    _logger.LogDebug("The data in the sendQueue arrived.");
                }
            });
        }

        public bool SendBuffer(byte[] buffer, bool activateTimer = false)
        {
            if (_disconnected) return false;
            try
            {
                if (_netStream.CanWrite)
                {
                    _netStream.Write(buffer, 0, buffer.Length);
                    _logger.LogDebug("SendBuffer() netStream.Write done.");
                    _respTimer.Enabled = activateTimer;
                    ResetPinger();
                    return true;
                }
                _logger.LogDebug("SendBuffer() netStream is null or not writable.");
                return false;
            }
            //client disconnected
            catch (Exception ex) when (ex is IOException || ex is ObjectDisposedException) { }
            return false;
        }

        public void DisconnectAndRaise()
        {
            if (_disconnected) return;
            _logger.LogDebug("Disconnecting client " + ClientData.ClientID.ToString() + " and raising event.", true);
            Disconnect();
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        public void Disconnect()
        {
            if (_disconnected) return;
            _disconnected = true;
            _logger.LogDebug("Disconnect()");

            _are.Set();
            _pingTimer.Dispose();
            _respTimer.Dispose();
            _tcpClient.Close();
            _tcpClient.Dispose();

            _logger.LogDebug("Disconnect() DONE.", true);
        }
    }
}
