using System;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1;
using Serilog;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factories
{
    public static class MessageFromMcuFactories
    {
        public static IFromMcuMessages GetDefault(ILogger logger)
        {
            return Create(logger);
        }

        public static IFromMcuMessages Create(ILogger logger, string factory = "ArduinoV1")
        {
            return factory switch
            { 
                "ArduinoV1" => new ArduinoV1Messages(logger),
                _ => throw new NotImplementedException($"FromMcuCommandsFactory '{factory}' not implemented.")
            };
        }
    }
}
