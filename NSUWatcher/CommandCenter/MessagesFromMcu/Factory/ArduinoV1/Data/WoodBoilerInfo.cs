using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
#nullable enable
    public class WoodBoilerInfo : IWoodBoilerInfo
    {
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.WoodBoiler.CurrentTemp)]
        public double CurrentTemperature { get; set; }
        
        [JsonProperty(JKeys.Generic.Status)]
        public string WBStatus { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.WoodBoiler.TemperatureStatus)]
        public string TempStatus { get; set; } = string.Empty;
    }
#nullable disable
}
