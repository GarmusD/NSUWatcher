using Newtonsoft.Json.Linq;
using NSUWatcher.Interfaces.MCUCommands;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1
{
    public interface IFromArduinoV1Base
    {
        public IMessageFromMcu? TryFindMessage(JObject command);
    }
}
