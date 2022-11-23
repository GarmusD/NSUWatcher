using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ITempChanged : IMessageFromMcu
    {
        [JsonProperty(JKeys.TempSensor.SensorID)]
        public string Address { get; set; }
        [JsonProperty(JKeys.TempSensor.Temperature)]
        public float Temperatur { get; set; }
        [JsonProperty(JKeys.TempSensor.ReadErrors)]
        public int ReadErrorCount { get; set; }
    }
}
