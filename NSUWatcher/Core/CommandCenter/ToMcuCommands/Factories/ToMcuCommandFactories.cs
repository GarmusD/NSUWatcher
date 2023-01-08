using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.Core.CommandCenter.ToMcuCommands.Factories.ArduinoV1;
using System;

namespace NSUWatcher.Core.CommandCenter.ToMcuCommands.Factories
{
    public static class ToMcuCommandFactories
    {
        public static IToMcuCommands GetDefault(Action<string> sendAction)
        {
            return CreateFactory(sendAction);
        }

        public static IToMcuCommands CreateFactory(Action<string> sendAction, string factoryName = "ArduinoV1")
        {
            return factoryName switch
            {
                "ArduinoV1" => new ArduinoV1Commands(sendAction),
                _ => throw new NotImplementedException($"MCU Command factory '{factoryName}' not implemented.")
            };
            ;
        }
    }
}
