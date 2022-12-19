using Newtonsoft.Json.Linq;
using NSU.Shared;
using NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data;
using NSUWatcher.Interfaces.MCUCommands;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1
{
    public class TempTriggerMessages : IFromArduinoV1Base
    {
        public IMessageFromMcu TryFindMessage(JObject command)
        {
            string source = (string)command[JKeys.Generic.Source];
            if (source != JKeys.TempTrigger.TargetName) return null;

            string action = (string)command[JKeys.Generic.Action];
            return action switch
            {
                JKeys.Action.Snapshot => command.ToObject<TempTriggerSnapshot>(),
                JKeys.Action.Info => command.ToObject<TempTriggerInfo>(),
                _ => null,
            };
        }
    }
}
