using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
#nullable enable
    public class SystemStatus : ISystemStatus
    {
        [JsonProperty(JKeys.Generic.Value)]
        public string CurrentState { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.Syscmd.FreeMem)]
        public int FreeMem { get; set; }
        
        [JsonProperty(JKeys.Syscmd.UpTime)]
        public int? UpTime { get; set; }
        
        [JsonProperty(JKeys.Syscmd.RebootRequired)]
        public bool RebootRequired { get; set; }
        
        //[JsonProperty(JKeys.Generic.Source)]
        //public string Source { get; set; } = string.Empty;
        
        //[JsonProperty(JKeys.Generic.CommandID)]
        //public string? CommandID { get; set; }
    }
#nullable disable
}
