using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
#nullable enable
    public class RelayStatus : IRelayInfo
    {
        [JsonProperty(JKeys.Generic.Value)]
        public IRelayModuleStatus[] Values { get; set; } = new IRelayModuleStatus[0];
        
        //[JsonProperty(JKeys.Generic.Source)]
        //public string Source { get; set; } = string.Empty;
        
        //[JsonProperty(JKeys.Generic.CommandID)]
        //public string? CommandID { get; set; }
    }
#nullable disable

    public class RelayModuleStatus : IRelayModuleStatus
    {
        [JsonProperty(JKeys.RelayModule.StatusFlags)]
        public byte StatusFlags { get; set; }
        
        [JsonProperty(JKeys.RelayModule.LockFlags)]
        public byte LockFlags { get; set; }
    }
}
