using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class SystemError : ISystemError
    {
        [JsonProperty(JKeys.Generic.Value)]
        public string ErrorValue { get; set; }
    }
}
