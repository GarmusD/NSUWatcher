using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
#nullable enable
    public class TempChanged : ITempChanged
    {
        [JsonProperty(JKeys.TempSensor.SensorID)]
        public string Address { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.TempSensor.Temperature)]
        public float Temperature { get; set; }
        
        [JsonProperty(JKeys.TempSensor.ReadErrors)]
        public int ReadErrorCount { get; set; }

        [JsonProperty(JKeys.Generic.CommandID)]
        public string? CommandID { get; set; }
    }
#nullable disable
}
