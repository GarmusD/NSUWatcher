using Newtonsoft.Json.Linq;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data;
using NSU.Shared;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1
{
    public class RelayMessages : IFromArduinoV1Base
    {
        public IMessageFromMcu TryFindMessage(JObject command)
        {
            string source = (string)command[JKeys.Generic.Source];
            if (source != JKeys.RelayModule.TargetName) return null;

            string action = (string)command[JKeys.Generic.Action];
            switch (action)
            {
                case JKeys.Action.Snapshot: return command.ToObject<RelaySnapshot>();
                case JKeys.RelayModule.ActionOpenChannel: return command.ToObject<RelayChannelOpened>();
                case JKeys.RelayModule.ActionCloseChannel: return command.ToObject<RelayChannelClosed>();
                case JKeys.Generic.Status: return command.ToObject<RelayStatus>();
                default: return null;
            };
        }
    }
}
