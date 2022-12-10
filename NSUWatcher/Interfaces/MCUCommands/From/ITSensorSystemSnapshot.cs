using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ITSensorSystemSnapshot : IMessageFromMcu
    {
        // Address in format "%02X:%02X:%02X:%02X:%02X:%02X:%02X:%02X"
        public string Address { get; set; }
        [JsonProperty(JKeys.TempSensor.Temperature)]
        public double Temperature { get; set; }
        [JsonProperty(JKeys.TempSensor.ReadErrors)]
        public int ReadErrors { get; set; }
    }
}
