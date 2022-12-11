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
