using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class RelayChannelOpened : IRelayChannelOpened
    {
        [JsonProperty(JKeys.Generic.Value)]
        public byte Channel { get; set; }
        
        [JsonProperty(JKeys.RelayModule.IsLocked)]
        public bool IsLocked { get; set; }

        [JsonProperty(JKeys.Generic.CommandID)]
        public string CommandID { get; set; }
    }
}
