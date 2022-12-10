using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    /// <summary>
    /// Contains status of all relay modules
    /// </summary>
    public interface IRelayInfo : IMessageFromMcu
    {
        [JsonProperty(JKeys.Generic.Value)]
        IRelayModuleStatus[] Values { get; set; }
    }

    /// <summary>
    /// Status of single relay module
    /// </summary>
    public interface IRelayModuleStatus
    {
        [JsonProperty(JKeys.RelayModule.StatusFlags)]
        byte StatusFlags { get; set; }
        [JsonProperty(JKeys.RelayModule.LockFlags)]
        byte LockFlags { get; set; }
    }
}
