using Newtonsoft.Json.Linq;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data;
using NSU.Shared;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1
{
    public class CircPumpMessages : IFromArduinoV1Base
    {
        public IMessageFromMcu TryFindMessage(JObject command)
        {
            string source = (string)command[JKeys.Generic.Source];
            if (source != JKeys.CircPump.TargetName) return null;

            string action = (string)command[JKeys.Generic.Action];
            return action switch
            {
                JKeys.Action.Snapshot => command.ToObject<CircPumpSnapshot>(),
                JKeys.Action.Info => command.ToObject<CircPumpInfo>(),
                _ => null,
            };
        }
    }
}
