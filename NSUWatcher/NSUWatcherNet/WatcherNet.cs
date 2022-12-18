using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.NsuUsers;
using NSUWatcher.NSUWatcherNet.NetMessenger;
using System.Threading;
using System.Threading.Tasks;

namespace NSUWatcher.NSUWatcherNet
{
    public class WatcherNet : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly NetServer _netServer;
        private readonly Messenger _netMessenger;

        public WatcherNet(ICmdCenter commandCenter, INsuSystem nsuSystem, INsuUsers nsuUsers, IConfiguration config, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory?.CreateLoggerShort<WatcherNet>() ?? NullLoggerFactory.Instance.CreateLoggerShort<WatcherNet>();
            _netServer = new NetServer(commandCenter, nsuUsers, nsuSystem, config, loggerFactory);
            _netMessenger = new Messenger(_netServer, nsuUsers, commandCenter, nsuSystem, loggerFactory);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _netServer.ExecuteAsync(stoppingToken);
        }
    }
}
