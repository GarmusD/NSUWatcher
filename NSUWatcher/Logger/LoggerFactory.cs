using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using System;
using System.IO;
using TelegramSink;

namespace NSUWatcher.Logger
{
    public static class LoggerFactory
    {
        private const string LogDir = "/var/log/nsuwatcher/";
        private const string OutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";
        public static LoggingLevelSwitch LoggingLevelSwitch { get; } = new LoggingLevelSwitch();
        public static LoggingLevelSwitch ConsoleLevelSwitch { get; } = new LoggingLevelSwitch(initialMinimumLevel: global::Serilog.Events.LogEventLevel.Debug);

        public static ILogger Create(IConfigurationRoot config, Action<LoggerConfiguration>? action = null)
        {
            var loggerConf = CreateDefaultConfiguration(config);
            action?.Invoke(loggerConf);
            Log.Logger = loggerConf.CreateLogger();
            return Log.Logger;
        }

        public static ILogger CreateWithConsole(IConfigurationRoot config)
        {
            var loggerConf = CreateDefaultConfiguration(config);
            //loggerConf.WriteTo.Console(levelSwitch: ConsoleLevelSwitch);
            loggerConf.WriteTo.Console(restrictedToMinimumLevel: global::Serilog.Events.LogEventLevel.Verbose, outputTemplate: OutputTemplate);
            Log.Logger = loggerConf.CreateLogger();
            return Log.Logger;
        }

        private static LoggerConfiguration CreateDefaultConfiguration(IConfigurationRoot config)
        {
            FileInfo logFile = SetupLogDir();
            var loggerConf = new LoggerConfiguration();
            
            loggerConf.ReadFrom.Configuration(config);            

            // Override MinimumLevel to Sinks because "NsuSinkConsole" needs to have Verbose or Debug level
            loggerConf.MinimumLevel.Verbose();
            
            
            //LoggingLevelSwitch.MinimumLevel = GetLevelFromConfig(config);
            //loggerConf.WriteTo.File(logFile.FullName, rollingInterval: RollingInterval.Day, shared: true, levelSwitch: LoggingLevelSwitch);
            
            // For now lets use "Serilog:MinimumLevel" config value for File sink
            loggerConf.WriteTo.File(logFile.FullName, rollingInterval: RollingInterval.Day, shared: true, restrictedToMinimumLevel: GetLevelFromConfig(config), outputTemplate: OutputTemplate);
            var teleLogCfg = config.GetSection("TelegramLog").Get<TelegramLogCfg>();
            if (teleLogCfg != null && teleLogCfg.Enabled)
            {
                loggerConf.WriteTo.TeleSink(teleLogCfg.BotToken, teleLogCfg.ChannelId.ToString(), minimumLevel: teleLogCfg.MinimumLevel);
            }
            loggerConf.Enrich.FromLogContext();
            return loggerConf;
        }

        private static FileInfo SetupLogDir()
        {
            if (!Directory.Exists(LogDir))
                Directory.CreateDirectory(LogDir);
            return new FileInfo(Path.Combine(LogDir, "log-.txt"));
        }

        private static global::Serilog.Events.LogEventLevel GetLevelFromConfig(IConfiguration config)
        {
            return config["Serilog:MinimumLevel"]?.ToLower() switch
            { 
                "verbose" => global::Serilog.Events.LogEventLevel.Verbose,
                "debug" => global::Serilog.Events.LogEventLevel.Debug,
                "information" => global::Serilog.Events.LogEventLevel.Information,
                "warning" => global::Serilog.Events.LogEventLevel.Warning,
                "error" => global::Serilog.Events.LogEventLevel.Error,
                "fatal" => global::Serilog.Events.LogEventLevel.Fatal,
                _ => global::Serilog.Events.LogEventLevel.Information
            };
        }
    }
}
