using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
#nullable enable
    public class CircPumpInfo : ICircPumpInfo
    {
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.Generic.Status)]
        public string Status { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.CircPump.CurrentSpeed)]
        public int CurrentSpeed { get; set; }
        
        [JsonProperty(JKeys.CircPump.ValvesOpened)]
        public int ValvesOpened { get; set; }
    }
#nullable disable
}
