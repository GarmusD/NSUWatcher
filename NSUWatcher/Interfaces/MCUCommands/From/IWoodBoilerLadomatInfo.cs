using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IWoodBoilerLadomatInfo : IMessageFromMcu
    {
        [JsonProperty(JKeys.Generic.Name)]
        string Name { get; set; }
        [JsonProperty(JKeys.Generic.Value)]
        string LadomatStatus { get; set; }
    }
}
