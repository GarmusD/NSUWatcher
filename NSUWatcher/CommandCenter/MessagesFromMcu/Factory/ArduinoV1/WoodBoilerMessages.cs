using Newtonsoft.Json.Linq;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data;
using NSU.Shared;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1
{
    public class WoodBoilerMessages : IFromArduinoV1Base
    {
        public IMessageFromMcu TryFindMessage(JObject command)
        {
            string source = (string)command[JKeys.Generic.Source];
            if (source != JKeys.WoodBoiler.TargetName) return null;

            string action = (string)command[JKeys.Generic.Action];
            switch (action)
            {
                case JKeys.Action.Snapshot: return command.ToObject<WoodBoilerSnapshot>();
                case JKeys.Action.Info: return GetCommandByContent(command);
                default: return null;
            };
        }

        private IMessageFromMcu GetCommandByContent(JObject command)
        {
            string content = (string)command[JKeys.Generic.Content];
            switch(content)
            {
                case JKeys.WoodBoiler.TargetName: return command.ToObject<WoodBoilerInfo>();
                case JKeys.WoodBoiler.LadomatStatus: return command.ToObject<WoodBoilerLadomatInfo>();
                case JKeys.WoodBoiler.ExhaustFanStatus: return command.ToObject<WoodBoilerExhaustFanInfo>();
                default: return null;
            };
        }
    }
}
