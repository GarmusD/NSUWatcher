using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ICollectorSnapshot : IMessageFromMcu
    {
        [JsonProperty(JKeys.Generic.ConfigPos)]
        public int ConfigPos { get; set; }
        [JsonProperty(JKeys.Generic.Enabled)]
        public bool Enabled { get; set; }
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; set; }
        [JsonProperty(JKeys.Collector.CircPumpName)]
        public string CircPumpName { get; set; }
        [JsonProperty(JKeys.Collector.ActuatorsCount)]
        public int ActuatorsCounts { get; set; }
        [JsonProperty(JKeys.Collector.Valves)]
        public ICollectorActuator[] Actuators { get; set; }
    }
}
