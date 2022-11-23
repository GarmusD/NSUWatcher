using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
	public interface IWoodBoilerSnapshot : IMessageFromMcu
    {
		[JsonProperty(JKeys.Generic.ConfigPos)]
		public int ConfigPos { get; set; }
		[JsonProperty(JKeys.Generic.Enabled)]
		public bool Enabled { get; set; }
		[JsonProperty(JKeys.Generic.Name)]
		public string Name { get; set; }
		[JsonProperty(JKeys.WoodBoiler.WorkingTemp)]
		public double WorkingTemperature { get; set; }
		[JsonProperty(JKeys.WoodBoiler.Histeresis)]
		public double Histeresis { get; set; }
		[JsonProperty(JKeys.WoodBoiler.TSensorName)]
		public string TempSensorName { get; set; }
		[JsonProperty(JKeys.WoodBoiler.KTypeName)]
		public string KTypeName { get; set; }
		[JsonProperty(JKeys.WoodBoiler.ExhaustFanChannel)]
		public int ExhaustFanChannel { get; set; }
		[JsonProperty(JKeys.WoodBoiler.LadomChannel)]
		public int LadomChannel { get; set; }
		[JsonProperty(JKeys.WoodBoiler.LadomatTriggerName)]
		public string LadomatTriggerName { get; set; }
		[JsonProperty(JKeys.WoodBoiler.LadomatWorkTemp)]
		public double LadomatWorkingTemp { get; set; }
		[JsonProperty(JKeys.WoodBoiler.WaterBoilerName)]
		public string WaterBoilerName { get; set; }
		[JsonProperty(JKeys.WoodBoiler.CurrentTemp)]
		public double? CurrentTemperature { get; set; }
		[JsonProperty(JKeys.Generic.Status)]
		public string? Status { get; set; }
		[JsonProperty(JKeys.WoodBoiler.LadomatStatus)]
		public string? LadomatStatus { get; set; }
		[JsonProperty(JKeys.WoodBoiler.ExhaustFanStatus)]
		public string? ExhaustFanStatus { get; set; }
		[JsonProperty(JKeys.WoodBoiler.TemperatureStatus)]
		public string? TemperatureStatus { get; set; }
    }
}
