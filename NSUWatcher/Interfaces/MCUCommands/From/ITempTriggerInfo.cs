using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ITempTriggerInfo : IMessageFromMcu
    {
        [JsonProperty(JKeys.Generic.Name)]
        string Name { get; set; }
        [JsonProperty(JKeys.Generic.Status)]
        string Status { get; set; }
    }
}
