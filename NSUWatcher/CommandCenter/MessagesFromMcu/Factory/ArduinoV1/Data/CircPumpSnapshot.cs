using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
#nullable enable
    public class CircPumpSnapshot : ICircPumpSnapshot
    {
        [JsonProperty(JKeys.Generic.ConfigPos)]
        public byte ConfigPos { get; set; }
        
        [JsonProperty(JKeys.Generic.Enabled)]
        public bool Enabled { get; set; }
        
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.CircPump.MaxSpeed)]
        public int MaxSpeed { get; set; }
        
        [JsonProperty(JKeys.CircPump.Speed1Ch)]
        public int Speed1Ch { get; set; }
        
        [JsonProperty(JKeys.CircPump.Speed2Ch)]
        public int Speed2Ch { get; set; }
        
        [JsonProperty(JKeys.CircPump.Speed3Ch)]
        public int Speed3Ch { get; set; }
        
        [JsonProperty(JKeys.CircPump.TempTriggerName)]
        public string TempTriggerName { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.Generic.Status)]
        public string Status { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.CircPump.CurrentSpeed)]
        public int? CurrentSpeed { get; set; }
        
        [JsonProperty(JKeys.CircPump.ValvesOpened)]
        public int? OpenedValvesCount { get; set; }
    }
#nullable disable
}
