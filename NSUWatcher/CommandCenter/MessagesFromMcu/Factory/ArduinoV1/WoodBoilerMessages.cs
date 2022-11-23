using Newtonsoft.Json.Linq;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data;
using NSU.Shared;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1
{
    public class WoodBoilerMessages : IFromArduinoV1Base
    {
        public IMessageFromMcu? TryFindMessage(JObject command)
        {
            string target = (string)command[JKeys.Generic.Target]!;
            if (target != JKeys.WoodBoiler.TargetName) return null;

            string action = (string)command[JKeys.Generic.Action]!;
            return action switch
            {
                JKeys.Action.Snapshot => command.ToObject<WoodBoilerSnapshot>(),
                JKeys.Action.Info => GetCommandByContent(command),
                _ => null
            };
        }

        private IMessageFromMcu? GetCommandByContent(JObject command)
        {
            string content = (string)command[JKeys.Generic.Content]!;
            return content switch
            {
                JKeys.WoodBoiler.TargetName => command.ToObject<WoodBoilerInfo>(),
                JKeys.WoodBoiler.LadomatStatus => command.ToObject<WoodBoilerLadomatInfo>(),
                JKeys.WoodBoiler.ExhaustFanStatus => command.ToObject<WoodBoilerExhaustFanInfo>(),
                _ => null
            };
        }
    }
}
