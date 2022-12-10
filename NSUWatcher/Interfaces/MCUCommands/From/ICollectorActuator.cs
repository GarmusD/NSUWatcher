using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ICollectorActuator
    {
        [JsonProperty(JKeys.Collector.ActuatorType)]
        public int ActuatorType { get; set; }
        [JsonProperty(JKeys.Collector.ActuatorChannel)]
        public int Channel { get; set; }
        [JsonProperty(JKeys.Generic.Status)]
        public bool? IsOpen { get; set; }
    }
}
