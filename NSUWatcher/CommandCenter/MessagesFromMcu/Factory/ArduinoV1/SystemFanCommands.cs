using Newtonsoft.Json.Linq;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data;
using NSU.Shared;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1
{
    public class SystemFanCommands : IFromArduinoV1Base
    {
        public IMessageFromMcu? TryFindMessage(JObject command)
        {
            string target = (string)command[JKeys.Generic.Target]!;
            if (target != JKeys.SystemFan.TargetName) return null;

            string action = (string)command[JKeys.Generic.Action]!;
            return action switch
            {
                JKeys.Action.Snapshot => command.ToObject<SystemFanSnapshot>(),
                JKeys.Action.Status => command.ToObject<SystemFanInfo>(),
                _ => null
            };
        }
    }
}
