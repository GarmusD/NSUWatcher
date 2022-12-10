using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IRelaySnapshot : IMessageFromMcu
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
}
