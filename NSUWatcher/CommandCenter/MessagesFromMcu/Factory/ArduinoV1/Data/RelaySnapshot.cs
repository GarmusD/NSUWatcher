using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
#nullable enable
    public class RelaySnapshot : IRelaySnapshot
    {
        [JsonProperty(JKeys.Generic.ConfigPos)]
        public byte ConfigPos { get; set; }
        
        [JsonProperty(JKeys.Generic.Enabled)]
        public bool Enabled { get; set; }
        
        [JsonProperty(JKeys.RelayModule.ActiveLow)]
        public bool ActiveLow { get; set; }
        
        [JsonProperty(JKeys.RelayModule.Reversed)]
        public bool Reversed { get; set; }
        
        [JsonProperty(JKeys.RelayModule.StatusFlags)]
        public byte? StatusFlags { get; set; }
        
        [JsonProperty(JKeys.RelayModule.LockFlags)]
        public byte? LockFlags { get; set; }
    }
#nullable disable
}
