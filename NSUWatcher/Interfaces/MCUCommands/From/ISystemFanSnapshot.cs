using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ISystemFanSnapshot : IMessageFromMcu
    {
		[JsonProperty(JKeys.Generic.ConfigPos)]
		public int ConfigPos { get; set; }
		[JsonProperty(JKeys.Generic.Enabled)]
		public bool Enabled { get; set; }
		[JsonProperty(JKeys.Generic.Name)]
		public string Name { get; set; }
		[JsonProperty(JKeys.SystemFan.TSensorName)]
		public string TempSensorName { get; set; }
		[JsonProperty(JKeys.SystemFan.MinTemp)]
		public double MinTemperature { get; set; }
		[JsonProperty(JKeys.SystemFan.MaxTemp)]
		public double MaxTemperature { get; set; }
		[JsonProperty(JKeys.Generic.Value)]
		public int? CurrentPWM { get; set; }
    }
}
