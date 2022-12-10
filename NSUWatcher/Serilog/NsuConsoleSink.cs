using Serilog.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using NSUWatcher.Serilog;

namespace NSUWatcher
{
    public class NsuConsoleSink : ILogEventSink
    {
        private readonly IFormatProvider? _formatProvider;

        public NsuConsoleSink(IFormatProvider? formatProvider)
        {
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            var message = logEvent.RenderMessage(_formatProvider);
            NsuConsoleMessage.SetOutoutLine(message);
        }
    }

    public static class MySinkExtensions
    {
        public static LoggerConfiguration NsuConsole(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IFormatProvider? formatProvider = null)
        {
            return loggerConfiguration.Sink(new NsuConsoleSink(formatProvider));
        }
    }
}
