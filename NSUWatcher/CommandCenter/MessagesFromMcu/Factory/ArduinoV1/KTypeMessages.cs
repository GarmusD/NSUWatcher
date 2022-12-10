using Newtonsoft.Json.Linq;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data;
using NSU.Shared;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1
{
    public class KTypeMessages : IFromArduinoV1Base
    {
        public IMessageFromMcu TryFindMessage(JObject command)
        {
            string source = (string)command[JKeys.Generic.Source];
            if (source != JKeys.KType.TargetName) return null;

            string action = (string)command[JKeys.Generic.Action];
            switch (action)
            {
                case JKeys.Action.Snapshot: return command.ToObject<KTypeSnapshot>();
                case JKeys.Action.Info: return command.ToObject<KTypeInfo>();
                default: return null;
            };
        }
    }
}
