using System;
using System.Timers;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.IO;
using NSU.Shared.NSUTypes;
using NSU.Shared.Compress;
using NSU.Shared;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using NSU.Shared.NSUNet;
using System.Net.NetworkInformation;
using System.Linq;

namespace NSUWatcher.NSUWatcherNet
{
    /// <summary>
    /// Network server to handle connection from app's
    /// </summary>
    public class NetServer
    {
        private const int SEND_BUFFER_HEADER_SIZE = sizeof(byte) + sizeof(int);
        private const int MAX_SEND_BUFFER_SIZE = 512 - SEND_BUFFER_HEADER_SIZE;

        readonly string LogTag = "NetServer";

        /// <summary>
        /// EventArgs for DataReceived event
        /// </summary>
        public class DataReceivedArgs : EventArgs
        {
            public NetClientData ClientData;
            public NetDataType datatype;
            public byte[] data;
            public int count;
            string s;

            
            public DataReceivedArgs(NetClientData ClientData, NetDataType dt, byte[] buff, int cnt)
            {
                this.ClientData = ClientData;
                datatype = dt;
                data = buff;
                count = cnt;
                s = string.Empty;
                if (datatype == NetDataType.String)
                {
                    s = Encoding.ASCII.GetString(data, 0, cnt);
                }
            }

            /// <summary>
            /// Only string data supported for now
            /// </summary>
            /// <param name="ClientData"></param>
            /// <param name="buff"></param>
            public DataReceivedArgs(NetClientData ClientData, string buff)
            {
                this.ClientData = ClientData;
                datatype = NetDataType.String;
                data = null;
                count = buff.Length;
                s = buff;
            }

            public string GetAsString()
            {
                return s;
            }
        };

        /// <summary>
        /// Helps to find out when network adapter is ready and connected
        /// </summary>
        private class AdapterAvailableHelper
        {
            private readonly string LogTag = "AdapterHelper";

            public event EventHandler OnAdapterAvailable;

            private System.Timers.Timer timer;

            public AdapterAvailableHelper()
            {
                timer = new System.Timers.Timer(5000);
                timer.Elapsed += HelperTimer_Elapsed;
                timer.AutoReset = true;
            }

            public void Stop()
            {
                timer.Enabled = false;
                timer.Dispose();
                timer = null;
            }

            public void WaitForAdapterReady()
            {
                if(!IsNetworkAdapterReady())
                {
                    timer.Enabled = true;
                }
                else
                {
                    timer.Enabled = false;
                    OnAdapterAvailable?.Invoke(this, null);
                }
            }

            /// <summary>
            /// Raise event if adapter is ready
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void HelperTimer_Elapsed(object sender, ElapsedEventArgs e)
            {
                if(IsNetworkAdapterReady())
                {
                    timer.Enabled = false;
                    OnAdapterAvailable?.Invoke(this, null);
                }
            }

            /// <summary>
            /// Check network adapter is connected
            /// </summary>
            /// <returns>true if connected</returns>
            private bool IsNetworkAdapterReady()
            {
                return NetworkInterface.GetIsNetworkAvailable();
            }
        }

        readonly Statistics _statistics = new Statistics();

        private TcpListener _server;
        //List of connected clients
        private readonly List<TcpClientHandler> _clients;
        private readonly SynchronizationContext _syncContext;
        private AdapterAvailableHelper _adapterHelper;
        private readonly object _slock;
        private volatile bool _stopRequest;
        private bool _runClientsCleanUp;
        //Command queue
        private readonly Queue<JObject> _cmdQueue = new Queue<JObject>();

        public delegate void ClientConnectedEventHandler(NetClientData clientData);
        public delegate void ClientDisconnectedEventHandler(NetClientData clientData);

        public event ClientConnectedEventHandler ClientConnected;
        public event ClientDisconnectedEventHandler ClientDisconnected;
        public event EventHandler<DataReceivedArgs> DataReceived;

        public bool Operational { get; private set; } = false;

        public NetServer(Config config)
        {
            _syncContext = SynchronizationContext.Current;
            _clients = new List<TcpClientHandler>();
            _server = new TcpListener(IPAddress.Any, config.NetServerPort);
            _server.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            _slock = new object();
            _adapterHelper = new AdapterAvailableHelper();
            _adapterHelper.OnAdapterAvailable += OnAdapterAvailable;            
        }

        private void OnAdapterAvailable(object sender, EventArgs e)
        {
            StartServerListening();
        }

        public void Start()
        {
            _adapterHelper.WaitForAdapterReady();
        }

        public void Stop()
        {
            NSULog.Debug(LogTag, "Stop()");
            NSULog.Debug(LogTag, "Stopping server...");
            _stopRequest = true;
            Operational = false;
            _adapterHelper.Stop();
            _adapterHelper = null;
            _server.Stop();
            NSULog.Debug(LogTag, "Disconnecting clients...");
            foreach (var item in _clients)
            {
                if (item.Connected)
                    item.Disconnect();
            }
            _clients.Clear();
            NSULog.Debug(LogTag, "NetServer stopped.");
        }

        private async Task StartServerListening()
        {
            while (!_stopRequest)
            {
                var result = await ListeningThread();
                if (result && !_stopRequest)//true - error;
                {
                    NSULog.Debug(LogTag, "Start(). await ListeningThread() returned error. Restarting listener.");
                    _clients.Clear();
                }
            }
        }

        private Task<bool> ListeningThread()
        {
            NSULog.Debug(LogTag, "Starting listener thread...");
            return Task.Run(() =>
            {
                bool res = false;//true - error; false - no error
                try
                {
                    _server.Start();
                    Operational = true;
                    NSULog.Debug(LogTag, "NetServer started. Waiting for clients...");
                    while (!_stopRequest)
                    {
                        try
                        {
                            TcpClient client = _server.AcceptTcpClient();
                            if (!_stopRequest)
                                ConnectClient(client);
                        }
                        catch (InvalidOperationException ex)
                        {
                            NSULog.Exception(LogTag, "ListeningThread():InvalidOperationException " + ex.Message);
                        }
                        catch (SocketException ex)
                        {
                            NSULog.Exception(LogTag, "ListeningThread():SocketException " + ex.Message);
                        }
                        catch (Exception ex)
                        {
                            NSULog.Exception(LogTag, "ListeningThread():Exception " + ex.Message);
                            res = true;
                            break;
                        }
                    }

                }
                catch (Exception ex)
                {
                    NSULog.Exception(LogTag, "ListeningThread(): server.Start() Exception - " + ex.Message);
                    res = true;
                }
                if (_server != null)
                {
                    _server.Stop();
                    _server = null;
                    Operational = false;
                }
                return res;
            });

        }

        private void ConnectClient(TcpClient client)
        {
            if (client != null)
            {
                var ch = new TcpClientHandler(client, Guid.NewGuid());
                ch.ClientData.IPAddress = (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString();
                ch.OnDataReceived += ClientOnDataReceivedHandler;
                ch.OnDisconnected += ClientOnDisconnected;                
                lock (_slock)
                {
                    _clients.Add(ch);
                }
                _ = ch.RunListener();
                NSULog.Debug(LogTag, "ListeningThread() - Raising ClientConnected event.");
                try
                {
                    if (_syncContext != null)
                    {
                        _syncContext.Post(RaiseClientConnectedEvent, ch.ClientData);
                    }
                    else
                    {
                        RaiseClientConnectedEvent(ch.ClientData);
                    }
                }
                catch (Exception ex)
                {
                    NSULog.Exception(LogTag, "ConnectClient()->RaiseClientConnectedEvent():Exception " + ex.Message);
                }
            }
        }

        private void ClientOnDataReceivedHandler(NetClientData clientData, InternalArgs args)
        {
            switch (args.dataType)
            {
                case NetDataType.Unknown:
                    break;
                case NetDataType.Binary:
                    break;
                case NetDataType.String:
                    if (args.data is string str && !string.IsNullOrWhiteSpace(str))
                    {
                        var evt = DataReceived;
                        evt?.Invoke(this, new DataReceivedArgs(clientData, str));
                    }
                    break;
                case NetDataType.CompressedString:
                    break;
                default:
                    break;
            }
        }

        private void ClientOnDisconnected(TcpClientHandler client)
        {
            NSULog.Debug(LogTag, $"ClientOnDisconnected(TcpClientHandler client - {client.ClientData.ClientID})");
            if (_stopRequest) return;
            try
            {
                var evt = ClientDisconnected;
                evt?.Invoke(client.ClientData);
            }
            finally
            {
                if (Monitor.IsEntered(_slock))
                    _runClientsCleanUp = true;
                else
                    CleanUpClients();
            }
        }

        private void RaiseClientConnectedEvent(object clientData)
        {
            ClientConnected?.Invoke(clientData as NetClientData);
        }

        private void CleanUpClients()
        {
            NSULog.Debug(LogTag, "CleanUpClients()", true);
            lock (_slock)
            {
                while (true)
                {
                    bool done = true;
                    foreach (var item in _clients)
                    {
                        if (!item.Connected)
                        {
                            DisconnectClient(item.ClientData);
                            done = false;
                            break;
                        }
                    }
                    if (done)
                        break;
                }
            }
            NSULog.Debug(LogTag, "CleanUpClients() DONE. Clients left: " + _clients.Count.ToString(), true);
        }

        public void DisconnectClient(NetClientData clientData)
        {
            NSULog.Debug(LogTag, $"DisconnectClient(NetClientData clientData - {clientData.ClientID})", true);
            if (clientData != null)
            {
                TcpClientHandler ch = GetByID(clientData.ClientID);
                if (ch != null)
                {
                    NSULog.Debug(LogTag, $"DisconnectClient(NetClientData clientData - {clientData.ClientID}):DisconnectClient(ch)", true);
                    DisconnectClient(ch);
                }
            }
        }

        private void DisconnectClient(TcpClientHandler ch)
        {
            NSULog.Debug(LogTag, "DisconnectClient(TcpClientHandler ch)", true);
            if (ch != null)
            {
                if(ch.Connected)
                    ch.Disconnect();
                NSULog.Debug(LogTag, "DisconnectClient(TcpClientHandler ch):clients.Remove(ch)", true);
                _clients.Remove(ch);
                NSULog.Debug(LogTag, string.Format("Disconnection done. Clients left: {0}", _clients.Count), true);
            }
            else
            {
                NSULog.Debug(LogTag, "DisconnectClient() ClientHandler is NULL", true);
            }
        }


        private TcpClientHandler GetByID(Guid guid)
        {
            return _clients.FirstOrDefault(x => x.ClientData.ClientID == guid);
        }

        public void AddCommand(JObject jo)
        {
            _cmdQueue.Enqueue(jo);
        }

        public void SendString(NetClientRequirements req, JObject data, string msg, bool activateTimer = false)
        {
            if (_clients.Count > 0)
            {
                string jstr;
                byte[] cbuff = null;
                byte[] buff = null;
                int jstrl = 0;

                if (data != null)
                {
                    jstr = JsonConvert.SerializeObject(data);
                    jstrl = jstr.Length * sizeof(char);
                    //NSULog.Debug(LogTag, "Preparing JSon to NetClient: " + jstr);
                    var compressed = StringCompressor.Zip(jstr);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.WriteByte((byte)NetDataType.CompressedString);
                        ms.Write(BitConverter.GetBytes(compressed.Length), 0, sizeof(int));
                        ms.Write(compressed, 0, compressed.Length);
                        ms.Flush();
                        cbuff = ms.ToArray();
                    }

                    _statistics.BeginSession(jstrl);

                    if (cbuff.Length <= MAX_SEND_BUFFER_SIZE)
                    {
                        DoSend(req, activateTimer, cbuff, buff);
                    }
                    else //Make parts and send
                    {
                        cbuff = MakePrtsAndSend(req, activateTimer, cbuff, buff);
                    }
                    _statistics.EndSession();
                    //NSULog.Debug(LogTag, "Total statistic: " + statistics.ToString());
                }
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    //NSULog.Debug(LogTag, "Preparing PlainText to NetClient: " + msg);
                    var b = Encoding.ASCII.GetBytes(msg);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.WriteByte((byte)NetDataType.String);
                        ms.Write(BitConverter.GetBytes(b.Length), 0, sizeof(int));
                        ms.Write(b, 0, b.Length);
                        ms.Flush();
                        buff = ms.ToArray();
                    }
                    _statistics.BeginSession(buff.Length);
                    DoSend(req, activateTimer, cbuff, buff);
                    _statistics.EndSession();
                    //NSULog.Debug(LogTag, "Total statistic: " + statistics.ToString());
                }
                if (_runClientsCleanUp)
                {
                    CleanUpClients();
                    _runClientsCleanUp = false;
                }
            }
        }

        private byte[] MakePrtsAndSend(NetClientRequirements req, bool activateTimer, byte[] cbuff, byte[] buff)
        {
            var startBuff = cbuff;
            var startL = startBuff.Length;
            var parts = startBuff.Length / MAX_SEND_BUFFER_SIZE;
            if ((startBuff.Length % MAX_SEND_BUFFER_SIZE) > 0)
                parts++;
            //NSULog.Debug(LogTag, $"Sending partial buffers... Parts count = {parts}");
            int i = 0;
            //NSULog.Debug(LogTag, $"Sending part init");
            using (MemoryStream ms = new MemoryStream())
            {
                ms.WriteByte((byte)NetDataType.PartialInit);
                ms.Write(BitConverter.GetBytes(sizeof(int)), 0, sizeof(int));
                ms.Write(BitConverter.GetBytes(startBuff.Length), 0, sizeof(int));
                ms.Flush();
                cbuff = ms.ToArray();
                DoSend(req, activateTimer, cbuff, buff);
            }

            while (i < parts)
            {
                //NSULog.Debug(LogTag, $"Sending part {i}");
                using (MemoryStream ms = new MemoryStream())
                {
                    var startPos = i * MAX_SEND_BUFFER_SIZE;
                    var leftL = startL - startPos;
                    leftL = leftL > MAX_SEND_BUFFER_SIZE ? MAX_SEND_BUFFER_SIZE : leftL;
                    //NSULog.Debug(LogTag, $"Bytes left in part: {leftL}");
                    ms.WriteByte((byte)NetDataType.Partial);
                    ms.Write(BitConverter.GetBytes(leftL), 0, sizeof(int));
                    ms.Write(startBuff, i * MAX_SEND_BUFFER_SIZE, leftL);
                    ms.Flush();
                    cbuff = ms.ToArray();
                    DoSend(req, activateTimer, cbuff, buff);
                }
                i++;
            }
            //NSULog.Debug(LogTag, $"Sending part done");
            using (MemoryStream ms = new MemoryStream())
            {
                ms.WriteByte((byte)NetDataType.PartialDone);
                ms.Write(BitConverter.GetBytes(sizeof(int)), 0, sizeof(int));
                ms.Write(BitConverter.GetBytes((int)1), 0, sizeof(int));
                ms.Flush();
                cbuff = ms.ToArray();
                DoSend(req, activateTimer, cbuff, buff);
            }

            return cbuff;
        }

        private void DoSend(NetClientRequirements req, bool activateTimer, byte[] cbuff, byte[] buff)
        {
            //var tmpClients = clients;
            //NSULog.Debug(LogTag, "DoSend()", true);
            lock (_slock)
            {
                foreach (TcpClientHandler clnt in _clients)
                {
                    try
                    {
                        if (clnt != null && clnt.Connected)
                        {
                            if (NetClientRequirements.Check(req, clnt.ClientData))
                            {
                                switch (clnt.ClientData.ProtocolVersion)
                                {
                                    case 1:
                                        if (buff != null)
                                        {
                                            //NSULog.Debug(LogTag, "Sending PlainText to " + clnt.ClientData.ClientID.ToString());
                                            if(clnt.SendBuffer(buff, 0, buff.Length, activateTimer))
                                            {
                                                _statistics.AddPerClient(buff.Length);
                                            }
                                        }
                                        break;
                                    case 2:
                                        if (cbuff != null)
                                        {
                                            //NSULog.Debug(LogTag, "DoSend():clnt.SendBuffer(): Sending JSon to " + clnt.ClientData.ClientID.ToString(), true);
                                            if (clnt.SendBuffer(cbuff, 0, cbuff.Length, activateTimer))
                                            {
                                                //NSULog.Debug(LogTag, "DoSend():clnt.SendBuffer(): Sending JSon OK.", true);
                                                _statistics.AddPerClient(cbuff.Length);
                                            }
                                            else
                                            {
                                                NSULog.Debug(LogTag, "DoSend():clnt.SendBuffer() returned false. Disconnecting client.", true);
                                                clnt.Disconnect();
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        NSULog.Exception(LogTag, "SendString() - " + ex.Message, true);
                        clnt.Disconnect();
                    }
                }
                _statistics.EndPerClient();
            }
        }

        public void SendBinaryData(NetClientRequirements req, byte[] data, int count = -1)
        {
            if (_clients.Count > 0)
            {
                byte[] buff = null;
                int c = count == -1 ? data.Length : count;
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.WriteByte((byte)NetDataType.Binary);
                    ms.Write(BitConverter.GetBytes(c), 0, sizeof(int));
                    ms.Write(data, 0, c);
                    ms.Flush();
                    buff = ms.GetBuffer();
                }
                if (buff == null) return;

                foreach (TcpClientHandler clnt in _clients)
                {
                    if (NetClientRequirements.Check(req, clnt.ClientData))
                    {
                        clnt.SendBuffer(buff);
                    }
                }
            }
        }


    }
}
