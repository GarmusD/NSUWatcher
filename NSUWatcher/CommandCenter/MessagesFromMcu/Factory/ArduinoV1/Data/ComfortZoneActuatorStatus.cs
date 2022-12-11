using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
#nullable enable
    public class ComfortZoneActuatorStatus : IComfortZoneActuatorStatus
    {
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.Generic.Value)]
        public bool Value { get; set; }
    }
#nullable disable
}
