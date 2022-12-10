using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands
{
    public interface IMessageFromMcu
    {
        [JsonProperty(JKeys.Generic.Target)]
        string Target { get; set; }
        [JsonProperty(JKeys.Generic.CommandID)]
        string? CommandID { get; set; }
    }
}
