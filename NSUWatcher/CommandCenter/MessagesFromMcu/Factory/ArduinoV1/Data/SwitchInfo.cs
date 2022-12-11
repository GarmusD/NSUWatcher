using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
#nullable enable
    public class SwitchInfo : ISwitchInfo
    {
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.Generic.Status)]
        public string Status { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.Switch.IsForced)]
        public bool IsForced { get; set; }
    }
#nullable disable
}
