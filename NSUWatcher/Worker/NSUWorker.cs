using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSUWatcher.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NSUWatcher.Worker
{
    public class NSUWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IMcuMessageTransport _mcuMessageTransport;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public NSUWorker(IMcuMessageTransport mcuMessageTransport, IHostApplicationLifetime lifetime, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory?.CreateLoggerShort<NSUWorker>() ?? NullLoggerFactory.Instance.CreateLoggerShort<NSUWorker>();
            _logger.LogDebug("Creating...");
            _mcuMessageTransport = mcuMessageTransport;
            _applicationLifetime = lifetime;
            _logger.LogTrace("Created.");
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace($"StartAsync()");
            return base.StartAsync(cancellationToken);// Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("StopAsync()");
            return base.StopAsync(cancellationToken);// Task.CompletedTask;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogDebug("ExecuteAsync(). Starting Transport...");
                if (!_mcuMessageTransport.Start())
                {
                    _logger.LogCritical("IMcuMessageTransport not started.");
                    _applicationLifetime.StopApplication();
                }

                _logger.LogDebug("ExecuteAsync(). Await stoppingToken...");
                TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
                var reg = stoppingToken.Register(() => completionSource.SetResult(true));
                await Task.WhenAny(completionSource.Task);
                _logger.LogDebug("ExecuteAsync(). Stop requested...");
            }
            catch (OperationCanceledException)
            {
            }

            _logger.LogDebug("ExecuteAsync(). Stopping Transport...");
            _mcuMessageTransport.Stop();            

            _logger.LogDebug("ExecuteAsync(). Done.");
        }
    }
}
