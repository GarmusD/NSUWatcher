using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IWoodBoilerExhaustFanInfo : IMessageFromMcu
    {
        [JsonProperty(JKeys.Generic.Name)]
        string Name { get; set; }
        [JsonProperty(JKeys.Generic.Value)]
        string ExhaustFanStatus { get; set; }
    }
}
