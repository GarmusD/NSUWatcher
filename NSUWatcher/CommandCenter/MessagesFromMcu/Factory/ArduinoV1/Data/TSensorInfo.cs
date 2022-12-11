using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
#nullable enable
    public class TSensorInfo : ITSensorInfo
    {
        [JsonProperty(JKeys.TempSensor.SensorID)]
        public string SensorID { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.Generic.Value)]
        public double Temperature { get; set; }
        
        [JsonProperty(JKeys.TempSensor.ReadErrors)]
        public int ReadErrorCount { get; set; }
        
        //[JsonProperty(JKeys.Generic.Source)]
        //public string Source { get; set; } = string.Empty;
        
        //[JsonProperty(JKeys.Generic.CommandID)]
        //public string? CommandID { get; set; }
    }
#nullable disable
}
