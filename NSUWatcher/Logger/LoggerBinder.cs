using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.CommandLine.Binding;
using System.IO;
using TelegramSink;

namespace NSUWatcher.Logger
{
    public class LoggerBinder : BinderBase<ILogger>
    {
        private const string LogDir = "/var/log/nsuwatcher/";

        private readonly ILogger _logger;

        private LoggerBinder(ILogger logger)
        {
            _logger = logger;
        }

        protected override ILogger GetBoundValue(BindingContext bindingContext) => _logger;

        public static LoggerBinder Configure(IConfigurationRoot config, Action<LoggerConfiguration>? action = null)
        {
            var loggerConf = CreateDefaultConfiguration(config);
            action?.Invoke(loggerConf);
            Log.Logger = loggerConf.CreateLogger();
            return new LoggerBinder(Log.Logger);
        }

        public static LoggerBinder ConfigureWithConsole(IConfigurationRoot config)
        {
            var loggerConf = CreateDefaultConfiguration(config);
            loggerConf.WriteTo.Console();
            Log.Logger = loggerConf.CreateLogger();
            return new LoggerBinder(Log.Logger);
        }

        private static LoggerConfiguration CreateDefaultConfiguration(IConfigurationRoot config)
        {
            FileInfo logFile = SetupLogDir();
            var loggerConf = new LoggerConfiguration();
            loggerConf.MinimumLevel.Debug();
            loggerConf.ReadFrom.Configuration(config);
            loggerConf.Enrich.FromLogContext();
            loggerConf.WriteTo.File(logFile.FullName, rollingInterval: RollingInterval.Day, shared: true);
            var teleLogCfg = config.GetSection("TelegramLog").Get<TelegramLogCfg>();
            if (teleLogCfg.Enabled)
            {
                loggerConf.WriteTo.TeleSink(teleLogCfg.BotToken, teleLogCfg.ChannelId.ToString(), minimumLevel: teleLogCfg.MinimumLevel);
            }
            return loggerConf;
        }

        private static FileInfo SetupLogDir()
        {
            if (!Directory.Exists(LogDir))
                Directory.CreateDirectory(LogDir);
            return new FileInfo(Path.Combine(LogDir, "log-.txt"));
        }
    }
}
