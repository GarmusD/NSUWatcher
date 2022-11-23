using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ISystemError : IMessageFromMcu
    {
        [JsonProperty(JKeys.Generic.Value)]
        string ErrorValue { get; set; }
    }
}
