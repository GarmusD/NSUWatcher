using Newtonsoft.Json.Linq;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data;
using NSU.Shared;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1
{
    public class SystemMessages : IFromArduinoV1Base
    {
        public IMessageFromMcu? TryFindMessage(JObject command)
        {
            string target = (string)command[JKeys.Generic.Target]!;
            if(target != JKeys.Syscmd.TargetName) return null;

            string action = (string)command[JKeys.Generic.Action]!;
            return action switch 
            {
                JKeys.Syscmd.SystemStatus => command.ToObject<SystemStatus>(),
                JKeys.Syscmd.Snapshot => command.ToObject<SystemSnapshotDone>(),
                _ => null
            };
        }

    }
}
