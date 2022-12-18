using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
#nullable enable
    public class SwitchSnapshot : ISwitchSnapshot
    {
        [JsonProperty(JKeys.Generic.ConfigPos)]
        public byte ConfigPos { get; set; }
        
        [JsonProperty(JKeys.Generic.Enabled)]
        public bool Enabled { get; set; }
        
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.Switch.Dependancy)]
        public string DependOnName { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.Switch.OnDependancyStatus)]
        public string DependancyStatus { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.Switch.ForceStatus)]
        public string ForceState { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.Switch.IsForced)]
        public bool? IsForced { get; set; }
        
        [JsonProperty(JKeys.Switch.CurrState)]
        public string? CurrentState { get; set; }
        
        //[JsonProperty(JKeys.Generic.Source)]
        //public string Source { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.Generic.CommandID)]
        public string? CommandID { get; set; }
    }
#nullable disable
}
