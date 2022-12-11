using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands
{
    // Empty interface to declare McuData
    public interface ICommandToMcuData
    {
        [JsonProperty(JKeys.Generic.Target)]
        string Target { get; }
        [JsonProperty(JKeys.Generic.Action)]
        string Action { get; }
    }
}
