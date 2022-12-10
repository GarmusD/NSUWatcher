using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ITSensorInfo : IMessageFromMcu
    {
        [JsonProperty(JKeys.TempSensor.SensorID)]
        string SensorID { get; set; }
        [JsonProperty(JKeys.Generic.Value)]
		double Temperature { get; set; }
        [JsonProperty(JKeys.TempSensor.ReadErrors)]
        int ReadErrorCount { get; set; }
    }
}
