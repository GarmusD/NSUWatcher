using Newtonsoft.Json.Linq;
using NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data;
using NSUWatcher.Interfaces.MCUCommands;
using static NSU.Shared.JKeys;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1
{
    public class AllarmMessages : IFromArduinoV1Base
    {
        public IMessageFromMcu TryFindMessage(JObject command)
        {
            string source = (string)command[Generic.Source];
            if (source != Alarm.TargetName) return null;

            string action = (string)command[Generic.Action];
            return action switch
            {
                Action.Snapshot => command.ToObject<AlarmSnapshot>(),
                _ => null,
            };
        }
    }
}
