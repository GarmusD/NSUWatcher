using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using NSUWatcher.Interfaces;
using NSUWatcher.NSUWatcherNet.NetMessenger;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace NSUWatcher.NSUWatcherNet
{
    /// <summary>
    /// Network server to handle connection from app's
    /// </summary>
    public partial class NetServer
    {
        
        //TODO Fix events
        public delegate void ClientConnectedEventHandler(NetClientData clientData);
        public delegate void ClientDisconnectedEventHandler(NetClientData clientData);

        public event ClientConnectedEventHandler ClientConnected;
        public event ClientDisconnectedEventHandler ClientDisconnected;

        private readonly ILogger _logger;
        private readonly TcpListener _server;
        private readonly List<NetClient> _clients;
        private readonly Messenger _netMessenger;
        private readonly object _slock = new object();
        private bool _stopRequested = false;

        public NetServer(ICmdCenter commandCenter, INsuSystem nsuSystem, IConfiguration config, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory?.CreateLoggerShort<NetServer>() ?? NullLoggerFactory.Instance.CreateLoggerShort<NetServer>();

            var netServerCfg = config.GetSection("netServer").Get<NetServerCfg>();
            if (netServerCfg.Port < 0)
            {
                _logger.LogInformation("The Port value in \"netServer\" section is less than 0. NetServer is disabled");
                return;
            }
            _logger.LogInformation($"Starting NetServer on port {netServerCfg.Port}. To disable NetServer set port value less than 0.");

            _server = new TcpListener(IPAddress.Any, netServerCfg.Port);
            _server.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            _clients = new List<NetClient>();
            _netMessenger = new Messenger(this, commandCenter, nsuSystem, loggerFactory);
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_server != null)
            {
                stoppingToken.Register(Stop);
                // Run until cancelled
                await RunServer(stoppingToken);
            }
        }

        public void Stop()
        {
            _logger.LogDebug("Stop() called. Stopping NetServer...");
            _stopRequested = true;

            _logger.LogDebug("Disconnecting clients...");
            foreach (var client in _clients)
            {
                if (client.Connected) client.Disconnect();
            }
            _clients.Clear();
            _server.Stop();
            _logger.LogDebug("NetServer stopped.");
        }

        private async Task RunServer(CancellationToken stoppingToken)
        {
            if (!WaitForNetworkAdapter(stoppingToken))
            {
                _logger.LogError("Network adapter is not ready. Terminating NetServer.");
                return;
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _server.Start();
                }
                catch (SocketException ex)
                {
                    _logger.LogError($"NetServer cannot start: {ex}");
                    return;
                }
                await StartServerListening();
            }
        }

        private async Task StartServerListening()
        {
            int retryCount = 0;
            DateTime startTime = DateTime.Now;
            while (true)
            {
                var stoppedOnError = await ListenForClientsAsync();
                if (DoNotRestartServer(ref retryCount, ref startTime, stoppedOnError))
                    return;
            }
        }

        private async Task<bool> ListenForClientsAsync()
        {
            // return values: true - error; false - no error
            try
            {
                _logger.LogDebug("NetServer is waiting for clients...");
                while (true)
                {
                    try
                    {
                        TcpClient client = await _server.AcceptTcpClientAsync();
                        ConnectClient(client);
                    }
                    catch (SocketException)
                    {
                        return false;
                    }
                    catch (ObjectDisposedException)
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ListenForClientsAsync(): Exception:");
                return true;
            }
        }

        private bool DoNotRestartServer(ref int retryCount, ref DateTime startTime, bool stoppedOnError)
        {
            if (_stopRequested) return true;

            if (stoppedOnError)
            {
                if (DateTime.Now - startTime < TimeSpan.FromSeconds(15))
                {
                    if (++retryCount > 5)
                    {
                        _logger.LogError($"Too much retries ({retryCount - 1}). NetServer will not be restarted.");
                        return true;
                    }
                }
                else
                {
                    retryCount = 0;
                }

                _logger.LogDebug("StartServerListening(): await ListeningThread() returned error. Restarting listener.");
                _clients.Clear();
                return false;
            }
            return true;
        }

        private bool WaitForNetworkAdapter(CancellationToken token)
        {
            bool netAdapterReady = false;
            using (var helper = new AdapterAvailableHelper(_logger))
            {
                // Blocking call until the adapter is ready or max retries or cancelled
                netAdapterReady = helper.WaitForAdapterReady(token);
            };
            return netAdapterReady;
        }

        private void ConnectClient(TcpClient tcpClient)
        {
            var client = new NetClient(tcpClient, _netMessenger, (ILogger<NetClient>)_logger);
            client.ClientData.IPAddress = !(tcpClient.Client.RemoteEndPoint is IPEndPoint endPoint) ? string.Empty : endPoint.Address.ToString();
            client.Disconnected += NetClientDisconnected;
            lock (_slock)
            {
                _clients.Add(client);
            }
            _ = client.StartListeningAsync();
            _logger.LogDebug("ConnectClient() - Raising ClientConnected event.");
            OnClientConnected(client.ClientData);
        }

        private void NetClientDisconnected(object sender, EventArgs e)
        {
            if (sender is NetClient client)
            {
                _logger.LogDebug($"ClientOnDisconnected(TcpClientHandler client - {client.ClientData.ClientID})");
                try
                {
                    var evt = ClientDisconnected;
                    evt?.Invoke(client.ClientData);
                }
                finally
                {
                    if (!Monitor.IsEntered(_slock))
                        CleanUpDisconnectedClients();
                }
            }
        }

        private void OnClientConnected(NetClientData clientData)
        {
            var evt = ClientConnected;
            evt?.Invoke(clientData);
        }

        private void CleanUpDisconnectedClients()
        {
            _clients.RemoveAll(x => x.Connected == false);
        }

        public void DisconnectClient(NetClientData clientData)
        {
            _logger.LogDebug($"DisconnectClient(NetClientData clientData - {clientData.ClientID})", true);
            NetClient netClient = GetByID(clientData.ClientID);
            if (netClient != null)
            {
                _logger.LogDebug($"DisconnectClient(NetClientData clientData - {clientData.ClientID}):DisconnectClient(ch)", true);
                DisconnectClient(netClient);
            }
        }

        private void DisconnectClient(NetClient netClient)
        {
            _logger.LogDebug("DisconnectClient(NetClient netClient)");
            if (netClient.Connected)
                netClient.Disconnect();
            _logger.LogDebug("DisconnectClient(NetClient netClient): _clients.Remove(netClient)");
            _clients.Remove(netClient);
            _logger.LogDebug($"Disconnection done. Clients left: {_clients.Count}");
        }


        private NetClient GetByID(Guid guid)
        {
            return _clients.FirstOrDefault(x => x.ClientData.ClientID == guid);
        }

        internal void Send(NetMessage netMessage, NetClientRequirements requirements)
        {
            //Filter clients
            var clientsToSend = _clients.Where(x => x.Connected && NetClientRequirements.Check(requirements, x.ClientData)).ToList();
            foreach (var client in clientsToSend)
            {
                client.Send(netMessage);
            }
        }
    }
}
