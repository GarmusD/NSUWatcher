using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
#nullable enable
    public class TSensorSystemSnapshot : ITSensorSystemSnapshot
    {
        [JsonProperty(JKeys.TempSensor.SensorID)]
        public string Address { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.TempSensor.Temperature)]
        public double Temperature { get; set; }
        
        [JsonProperty(JKeys.TempSensor.ReadErrors)]
        public int ReadErrors { get; set; }
    }
#nullable disable
}
