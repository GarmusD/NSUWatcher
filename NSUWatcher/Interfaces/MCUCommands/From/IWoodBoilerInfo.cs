using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
	public interface IWoodBoilerInfo : IMessageFromMcu
    {
		[JsonProperty(JKeys.Generic.Name)]
		string Name { get; set; }
		[JsonProperty(JKeys.WoodBoiler.CurrentTemp)]
		double CurrentTemperature { get; set; }
		[JsonProperty(JKeys.Generic.Status)]
		string WBStatus { get; set; }
		[JsonProperty(JKeys.WoodBoiler.TemperatureStatus)]
		string TempStatus { get; set; }
    }
}
