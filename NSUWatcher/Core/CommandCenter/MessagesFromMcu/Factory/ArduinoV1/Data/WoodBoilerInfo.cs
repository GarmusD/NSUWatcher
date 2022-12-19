using Newtonsoft.Json;
using NSU.Shared;
using NSU.Shared.NSUSystemPart;
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
        
        //[JsonProperty(JKeys.Generic.Source)]
        //public string Source { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.Generic.CommandID)]
        public string? CommandID { get; set; }
    }
#nullable disable
}
