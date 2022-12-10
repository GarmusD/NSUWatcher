using System;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1;
using Microsoft.Extensions.Logging;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factories
{
    public static class MessageFromMcuFactories
    {
        public static IFromMcuMessages GetDefault(ILoggerFactory loggerFactory)
        {
            return Create(loggerFactory);
        }

        public static IFromMcuMessages Create(ILoggerFactory loggerFactory, string factory = "ArduinoV1")
        {
            return factory switch
            {
                "ArduinoV1" => new ArduinoV1Messages(loggerFactory),
                _ => throw new NotImplementedException($"FromMcuCommandsFactory '{factory}' not implemented."),
            };
            ;
        }
    }
}
