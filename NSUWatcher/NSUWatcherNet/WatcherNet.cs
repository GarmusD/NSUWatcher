using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NSUWatcher.Interfaces;
using NSUWatcher.NSUWatcherNet.NetMessenger;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

namespace NSUWatcher.NSUWatcherNet
{
    public class WatcherNet : IHostedService
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly NetServer _netServer;
        private readonly Messenger _netMessenger;

        public WatcherNet(ICmdCenter commandCenter, INsuSystem nsuSystem, INsuUsers nsuUsers, IConfiguration config, ILogger logger)
        {
            _netServer = new NetServer(commandCenter, nsuSystem, config, logger);
            _netMessenger = new Messenger(_netServer, commandCenter, nsuSystem, logger);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _netServer.StartAsync();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _netServer.StopAsync();
            return Task.CompletedTask;
        }
    }
}
