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
        public byte CurrentSpeed { get; set; }
        
        [JsonProperty(JKeys.CircPump.ValvesOpened)]
        public byte ValvesOpened { get; set; }
        
        //[JsonProperty(JKeys.Generic.Source)]
        //public string Source { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.Generic.CommandID)]
        public string? CommandID { get; set; }
    }
#nullable disable
}
