using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
#nullable enable
    public class SystemFanSnapshot : ISystemFanSnapshot
    {
        [JsonProperty(JKeys.Generic.ConfigPos)]
        public byte ConfigPos { get; set; }
        
        [JsonProperty(JKeys.Generic.Enabled)]
        public bool Enabled { get; set; }
        
        [JsonProperty(JKeys.Generic.Name)]        
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.SystemFan.TSensorName)]
        public string TempSensorName { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.SystemFan.MinTemp)]
        public double MinTemperature { get; set; }
        
        [JsonProperty(JKeys.SystemFan.MaxTemp)]
        public double MaxTemperature { get; set; }
        
        [JsonProperty(JKeys.Generic.Value)]
        public int? CurrentPWM { get; set; }

        //[JsonProperty(JKeys.Generic.Source)]
        //public string Source { get; set; } = string.Empty;

        //[JsonProperty(JKeys.Generic.CommandID)]
        //public string? CommandID { get; set; }
    }
#nullable disable
}
