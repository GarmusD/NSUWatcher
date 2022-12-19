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
        public byte MaxSpeed { get; set; }
        
        [JsonProperty(JKeys.CircPump.Speed1Ch)]
        public byte Speed1Ch { get; set; }
        
        [JsonProperty(JKeys.CircPump.Speed2Ch)]
        public byte Speed2Ch { get; set; }
        
        [JsonProperty(JKeys.CircPump.Speed3Ch)]
        public byte Speed3Ch { get; set; }
        
        [JsonProperty(JKeys.CircPump.TempTriggerName)]
        public string TempTriggerName { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.Generic.Status)]
        public string Status { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.CircPump.CurrentSpeed)]
        public byte? CurrentSpeed { get; set; }
        
        [JsonProperty(JKeys.CircPump.ValvesOpened)]
        public byte? OpenedValvesCount { get; set; }
        
        //[JsonProperty(JKeys.Generic.Source)]
        //public string Source { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.Generic.CommandID)]
        public string? CommandID { get; set; }
    }
#nullable disable
}
