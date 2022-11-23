using Newtonsoft.Json;
using NSU.Shared;
using NSU.Shared.NSUSystemPart;
using System.Xml.Linq;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ICircPumpInfo : IMessageFromMcu
    {
        [JsonProperty(JKeys.Generic.Name)]
        string Name { get; set; }
        [JsonProperty(JKeys.Generic.Status)]
        string Status { get; set; }
        [JsonProperty(JKeys.CircPump.CurrentSpeed)]
        int CurrentSpeed { get; set; }
        [JsonProperty(JKeys.CircPump.ValvesOpened)]
        int ValvesOpened { get; set; }
    }
}
