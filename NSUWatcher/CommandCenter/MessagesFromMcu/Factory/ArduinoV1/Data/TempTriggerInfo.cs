using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class TempTriggerInfo : ITempTriggerInfo
    {
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; set; }

        [JsonProperty(JKeys.Generic.Status)]
        public string Status { get; set; }
    }
}
