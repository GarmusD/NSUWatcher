using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ICollectorInfo : IMessageFromMcu
    {
		[JsonProperty(JKeys.Generic.Name)]
		string Name { get; set; }
		[JsonProperty(JKeys.Generic.Status)]
		bool[] OpenedValves { get; set; }
	}
}
