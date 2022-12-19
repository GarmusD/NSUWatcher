using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
#nullable enable
    public class WoodBoilerExhaustFanInfo : IWoodBoilerExhaustFanInfo
    {
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.Generic.Value)]
        public string ExhaustFanStatus { get; set; } = string.Empty;
        
        //[JsonProperty(JKeys.Generic.Source)]
        //public string Source { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.Generic.CommandID)]
        public string? CommandID { get; set; } = string.Empty;
    }
#nullable disable
}
