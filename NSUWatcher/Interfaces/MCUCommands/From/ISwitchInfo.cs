using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ISwitchInfo : IMessageFromMcu
    {
        [JsonProperty(JKeys.Generic.Name)]
        string Name { get; set; }
        [JsonProperty(JKeys.Generic.Status)]
        string Status { get; set; }
        [JsonProperty(JKeys.Switch.IsForced)]
        bool IsForced { get; set; }
    }
}
