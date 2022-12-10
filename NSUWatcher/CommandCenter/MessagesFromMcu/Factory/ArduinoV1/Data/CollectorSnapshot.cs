using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
#nullable enable
    public class CollectorSnapshot : ICollectorSnapshot
    {
        [JsonProperty(JKeys.Generic.ConfigPos)]
        public byte ConfigPos { get; set; }
        
        [JsonProperty(JKeys.Generic.Enabled)]
        public bool Enabled { get; set; }
        
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.Collector.CircPumpName)]
        public string CircPumpName { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.Collector.ActuatorsCount)]
        public int ActuatorsCounts { get; set; }
        
        [JsonProperty(JKeys.Collector.Valves)]
        public ICollectorActuator[] Actuators { get; set; } = new CollectorActuator[0];
        
        //[JsonProperty(JKeys.Generic.Source)]
        //public string Source { get; set; } = string.Empty;
        
        //[JsonProperty(JKeys.Generic.CommandID)]
        //public string? CommandID { get; set; }
    }

    public class CollectorActuator : ICollectorActuator
    {
        [JsonProperty(JKeys.Collector.ActuatorType)]
        public int ActuatorType { get; set; }
        
        [JsonProperty(JKeys.Collector.ActuatorChannel)]
        public int Channel { get; set; }
        
        [JsonProperty(JKeys.Generic.Status)]
        public bool? IsOpen { get; set; }
    }
#nullable disable
}
