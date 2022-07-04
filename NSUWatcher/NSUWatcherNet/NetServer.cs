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

        private System.Timers.Timer netTimer;

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
        private class AdapterHelper
        {
            private readonly string LogTag = "AdapterHelper";

            public event EventHandler OnAdapterAvailable;

            private System.Timers.Timer timer;

            public AdapterHelper()
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

                //NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                //if (nics == null || nics.Length < 1)
                //{
                //    NSULog.Debug(LogTag, "No network interfaces found.");
                //    return false;
                //}
                //foreach (NetworkInterface adapter in nics)
                //{
                //    IPInterfaceProperties properties = adapter.GetIPProperties();
                //    if (adapter.Supports(NetworkInterfaceComponent.IPv4) &&
                //        adapter.OperationalStatus == OperationalStatus.Up &&
                //        (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet || adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                //    {
                //        return true;
                //    }
                //}
                //return false;
            }
        }

        readonly Statistics statistics = new Statistics();

        TcpListener server;
        //List of connected clients
        List<TcpClientHandler> clients;
        SynchronizationContext Context;
        AdapterHelper ahelper;
        object slock;
        volatile bool stop_request;
        private bool runClientsCleanUp;
        //Command queue
        private Queue<JObject> CmdQueue = new Queue<JObject>();

        public delegate void ClientConnectedEventHandler(NetClientData clientData);
        public delegate void ClientDisconnectedEventHandler(NetClientData clientData);

        public event ClientConnectedEventHandler ClientConnected;
        public event ClientDisconnectedEventHandler ClientDisconnected;
        public event EventHandler<DataReceivedArgs> OnDataReceived = delegate { };

        public bool Operational { get; private set; } = false;

        public NetServer()
        {
            Context = SynchronizationContext.Current;
            clients = new List<TcpClientHandler>();
            server = new TcpListener(IPAddress.Any, 5152);// ;(IPAddress.Parse("192.168.0.111"), 5152)
            server.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            slock = new object();
            ahelper = new AdapterHelper();
            ahelper.OnAdapterAvailable += OnAdapterAvailable;            
        }

        private void OnAdapterAvailable(object sender, EventArgs e)
        {
            StartServerListening();
        }

        public void Start()
        {
            ahelper.WaitForAdapterReady();
        }

        public void Stop()
        {
            NSULog.Debug(LogTag, "Stop()");
            NSULog.Debug(LogTag, "Stopping server...");
            stop_request = true;
            Operational = false;
            ahelper.Stop();
            ahelper = null;
            server.Stop();
            NSULog.Debug(LogTag, "Disconnecting clients...");
            foreach (var item in clients)
            {
                if (item.Connected)
                    item.Disconnect();
            }
            clients.Clear();
            clients = null;
            NSULog.Debug(LogTag, "NetServer stopped.");
        }

        private async void StartServerListening()
        {
            while (!stop_request)
            {
                var result = await ListeningThread();
                if (result && !stop_request)//true - error;
                {
                    NSULog.Debug(LogTag, "Start(). await ListeningThread() returned error. Restarting listener.");
                    clients.Clear();
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
                    server.Start();
                    Operational = true;
                    NSULog.Debug(LogTag, "NetServer started. Waiting for clients...");
                    while (!stop_request)
                    {
                        try
                        {
                            TcpClient client = server.AcceptTcpClient();
                            if (!stop_request)
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
                if (server != null)
                {
                    server.Stop();
                    server = null;
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
                lock (slock)
                {
                    clients.Add(ch);
                }
                _ = ch.RunListener();
                NSULog.Debug(LogTag, "ListeningThread() - Raising ClientConnected event.");
                try
                {
                    if (Context != null)
                    {
                        Context.Post(RaiseClientConnectedEvent, ch.ClientData);
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
                    string str = args.data as string;
                    if (str != null && !string.IsNullOrWhiteSpace(str))
                    {
                        //NSULog.Debug(LogTag, "Internal NetServer.ch_DataReceived(" + str + ")");
                        OnDataReceived?.Invoke(this, new DataReceivedArgs(clientData, str));
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
            if (stop_request) return;
            if (Monitor.IsEntered(slock))
                runClientsCleanUp = true;
            else
                CleanUpClients();
        }

        private void RaiseClientConnectedEvent(object clientData)
        {
            ClientConnected?.Invoke(clientData as NetClientData);
        }

        private void CleanUpClients()
        {
            NSULog.Debug(LogTag, "CleanUpClients()", true);
            lock (slock)
            {
                while (true)
                {
                    bool done = true;
                    foreach (var item in clients)
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
            NSULog.Debug(LogTag, "CleanUpClients() DONE. Clients left: " + clients.Count.ToString(), true);
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
                clients.Remove(ch);
                ch = null;
                NSULog.Debug(LogTag, string.Format("Disconnection done. Clients left: {0}", clients.Count), true);
            }
            else
            {
                NSULog.Debug(LogTag, "DisconnectClient() ClientHandler is NULL", true);
            }
        }


        private TcpClientHandler GetByID(Guid guid)
        {
            TcpClientHandler result = null;
            lock (slock)
            {
                foreach (var ch in clients)
                {
                    if (ch.ClientData.ClientID.Equals(guid))
                    {
                        result = ch;
                    }
                }
            }
            return result;
        }

        public void AddCommand(JObject jo)
        {
            CmdQueue.Enqueue(jo);
        }

        public void SendString(NetClientRequirements req, JObject data, string msg, bool activateTimer = false)
        {
            if (clients.Count > 0)
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

                    statistics.BeginSession(jstrl);

                    if (cbuff.Length <= MAX_SEND_BUFFER_SIZE)
                    {
                        DoSend(req, activateTimer, cbuff, buff);
                    }
                    else //Make parts and send
                    {
                        //statistics.BeginPartialMode();

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
                        //NSULog.Debug(LogTag, "Sending partial buffers DONE.");
                        //statistics.EndPartialMode();
                    }
                    statistics.EndSession();
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
                    statistics.BeginSession(buff.Length);
                    DoSend(req, activateTimer, cbuff, buff);
                    statistics.EndSession();
                    //NSULog.Debug(LogTag, "Total statistic: " + statistics.ToString());
                }
                if (runClientsCleanUp)
                {
                    CleanUpClients();
                    runClientsCleanUp = false;
                }
            }
        }

        private void DoSend(NetClientRequirements req, bool activateTimer, byte[] cbuff, byte[] buff)
        {
            //var tmpClients = clients;
            //NSULog.Debug(LogTag, "DoSend()", true);
            lock (slock)
            {
                foreach (TcpClientHandler clnt in clients)
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
                                                statistics.AddPerClient(buff.Length);
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
                                                statistics.AddPerClient(cbuff.Length);
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
                statistics.EndPerClient();
            }
        }

        public void SendBinaryData(NetClientRequirements req, byte[] data, int count = -1)
        {
            if (clients.Count > 0)
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

                foreach (TcpClientHandler clnt in clients)
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
