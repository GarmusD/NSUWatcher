using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSU.Shared;
using NSU.Shared.Compress;
using NSU.Shared.NSUNet;
using NSU.Shared.NSUTypes;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace NSUWatcher.NSUWatcherNet
{
    class TcpClientHandler
    {
        readonly string LogTag = "NetServer.TcpClientHandler";
        public const int PING_INTERVAL = 15 * 60;

        TcpClient client;
        NetworkStream netStream;
        InternalArgBuilder idra;
        System.Timers.Timer respTimer;
        System.Timers.Timer pingTimer;

        public delegate void DataReceivedEventHandler(NetClientData ClientData, InternalArgs args);
        public delegate void DisconnectedEventHandler(TcpClientHandler client);
        public event DataReceivedEventHandler OnDataReceived;
        public event DisconnectedEventHandler OnDisconnected;

        private volatile bool disconnected = false;

        public NetClientData ClientData { get; }

        public TcpClientHandler(TcpClient _client, Guid _ID)
        {
            client = _client;
            ClientData = new NetClientData();
            ClientData.ProtocolVersion = 2;
            ClientData.ClientID = _ID;
            ClientData.ClientType = NetClientData.NetClientType.Unknow;

            idra = new InternalArgBuilder();

            respTimer = new System.Timers.Timer();
            respTimer.Interval = 1000 * 15; //15sek to respond
            respTimer.Elapsed += RespTimer_Elapsed;
            respTimer.Start();

            pingTimer = new System.Timers.Timer();
            pingTimer.Interval = 1000 * 60 * 5;//5min
            pingTimer.Elapsed += PingTimer_Elapsed;
        }

        private void PingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            NSULog.Debug(LogTag, "PingTimer_Elapsed()");
            JObject jo = new JObject();
            jo[JKeys.Generic.Target] = JKeys.Syscmd.TargetName;
            jo[JKeys.Generic.Action] = "ping";
            SendString(JsonConvert.SerializeObject(jo));
        }

        private void RespTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //No response over time. Old client?
            NSULog.Debug(LogTag, "RespTimer_Elapsed()");
            DisconnectAndRaise();
        }

        private void ResetPinger()
        {
            pingTimer.Enabled = false;
            pingTimer.Enabled = true;
        }

        public bool Connected
        {
            get { return !disconnected && client != null && client.Connected; }
        }

        protected void RaiseDataReceived(NetClientData ClientData, InternalArgs args)
        {
            OnDataReceived?.Invoke(ClientData, args);
        }

        public async Task RunListener()
        {
            if (disconnected) return;
            await Task.Run(() =>
            {
                try
                {
                    if (client != null && client.Connected)
                    {
                        netStream = client.GetStream();
                        byte[] buffer = new byte[client.ReceiveBufferSize];
                        while (!disconnected && client != null && client.Connected)
                        {
                            if (netStream != null)
                            {
                                int count = netStream.Read(buffer, 0, buffer.Length);
                                if (count > 0)
                                {
                                    ResetPinger();
                                    respTimer.Enabled = false;
                                    idra.Process(buffer, 0, count);
                                    while (idra.DataAvailable)
                                    {
                                        RaiseDataReceived(ClientData, idra.GetArgs());
                                    }
                                }
                            }
                            else
                            {
                                NSULog.Debug(LogTag, "ListenerThread(). netStream Is NULL. Disconnecting...", true);
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    NSULog.Exception(LogTag, "ListenerThread()(Exception) - " + ex.Message, true);
                }
                if (netStream != null)
                {
                    netStream.Close();
                    netStream = null;
                }
                DisconnectAndRaise();
            });
        }

        public bool SendBuffer(byte[] buffer, int start = 0, int count = -1, bool activateTimer = false)
        {
            if (disconnected) return false;
            try
            {
                int c = count == -1 ? buffer.Length : count;
                if (netStream != null && netStream.CanWrite)
                {
                    netStream.Write(buffer, 0, c);
                    respTimer.Enabled = activateTimer;
                    ResetPinger();
                    return true;
                }
                else
                {
                    NSULog.Debug(LogTag, "SendBuffer() netStream is null or not writable.", true);
                    DisconnectAndRaise();
                }
            }
            catch (Exception e)
            {
                //client disconnected?
                NSULog.Exception(LogTag, "SendBuffer() exception. " + e.Message, true);
                DisconnectAndRaise();
            }
            return false;
        }

        public bool SendString(string jstr)
        {
            if (disconnected) return false;
            byte[] cbuff = null;

            var compressed = StringCompressor.Zip(jstr);
            using (MemoryStream ms = new MemoryStream())
            {
                ms.WriteByte((byte)NetDataType.CompressedString);
                ms.Write(BitConverter.GetBytes(compressed.Length), 0, sizeof(int));
                ms.Write(compressed, 0, compressed.Length);
                ms.Flush();
                cbuff = ms.ToArray();
            }
            return SendBuffer(cbuff);
        }

        public void DisconnectAndRaise()
        {
            if (disconnected) return;
            NSULog.Debug(LogTag, "Disconnecting client " + ClientData.ClientID.ToString() + " and raising event.", true);
            Disconnect();
            OnDisconnected?.Invoke(this);
        }

        public void Disconnect()
        {
            if (disconnected) return;
            disconnected = true;
            NSULog.Debug(LogTag, "Disconnect()", true);
            try
            {
                pingTimer.Enabled = false;
                respTimer.Enabled = false;
                pingTimer.Dispose();
                respTimer.Dispose();

                if (client != null)
                {
                    client.Close();
                }
            }
            finally
            {
                pingTimer = null;
                respTimer = null;
                netStream = null;
                client = null;
            }
            NSULog.Debug(LogTag, "Disconnect() DONE.", true);
        }
    }

}
