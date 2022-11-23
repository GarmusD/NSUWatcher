using Newtonsoft.Json.Linq;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data;
using NSU.Shared;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1
{
    public class TSensorsMessages : IFromArduinoV1Base
    {
        public IMessageFromMcu? TryFindMessage(JObject command)
        {
            string target = (string)command[JKeys.Generic.Target]!;
            if (target != JKeys.TempSensor.TargetName) return null;

            string action = (string)command[JKeys.Generic.Action]!;
            return action switch
            {
                JKeys.Action.Snapshot => GetSnapshotByContent(command),
                JKeys.Action.Info => command.ToObject<TSensorInfo>(),
                _ => null
            };
        }

        private IMessageFromMcu? GetSnapshotByContent(JObject command)
        {
            string content = (string)command[JKeys.Generic.Content]!;
            return content switch 
            { 
                JKeys.TempSensor.ContentSystem => command.ToObject<TSensorSystemSnapshot>(),
                JKeys.TempSensor.ContentConfig => command.ToObject<TSensorConfigSnapshot>(),
                _ => null 
            };
        }
    }
}
