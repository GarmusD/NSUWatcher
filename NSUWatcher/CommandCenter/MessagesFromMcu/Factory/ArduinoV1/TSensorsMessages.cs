using Newtonsoft.Json.Linq;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data;
using NSU.Shared;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1
{
    public class TSensorsMessages : IFromArduinoV1Base
    {
        public IMessageFromMcu TryFindMessage(JObject command)
        {
            string source = (string)command[JKeys.Generic.Source];
            if (source != JKeys.TempSensor.TargetName) return null;

            string action = (string)command[JKeys.Generic.Action];
            switch (action)
            {
                case JKeys.Action.Snapshot: return GetSnapshotByContent(command);
                case JKeys.Action.Info: return command.ToObject<TSensorInfo>();
                default: return null;
            };
        }

        private IMessageFromMcu GetSnapshotByContent(JObject command)
        {
            string content = (string)command[JKeys.Generic.Content];
            switch (content) 
            {
                case JKeys.TempSensor.ContentSystem: return command.ToObject<TSensorSystemSnapshot>();
                case JKeys.TempSensor.ContentConfig: return command.ToObject<TSensorConfigSnapshot>();
                default: return null;
            };
        }
    }
}
