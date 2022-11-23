using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
	
    public interface IWaterBoilerSnapshot : IMessageFromMcu
    {
		[JsonProperty(JKeys.Generic.ConfigPos)]
		public int ConfigPos { get; set; }
		[JsonProperty(JKeys.Generic.Enabled)]
		public bool Enabled { get; set; }
		[JsonProperty(JKeys.Generic.Name)]
		public string Name { get; set; }
		[JsonProperty(JKeys.WaterBoiler.TempSensorName)]
		public string TempSensorName { get; set; }
		[JsonProperty(JKeys.WaterBoiler.CircPumpName)]
		public string CircPumpName { get; set; }
		[JsonProperty(JKeys.WaterBoiler.TempTriggerName)]
		public string TempTriggerName { get; set; }
		[JsonProperty(JKeys.WaterBoiler.ElHeatingEnabled)]
		public bool ElHeatingEnabled { get; set; }
		[JsonProperty(JKeys.WaterBoiler.ElPowerChannel)]
		public int ElPowerChannel { get; set; }
		[JsonProperty(JKeys.WaterBoiler.PowerData)]
		public IElHeatingDataSnapshot[] ElHeatingData { get; set; }
	}
	
	public interface IElHeatingDataSnapshot
	{
		[JsonProperty(JKeys.WaterBoiler.PDStartHour)]
		public int StartHour { get; set; }
		[JsonProperty(JKeys.WaterBoiler.PDStartMin)]
		public int StartMinute { get; set; }
		[JsonProperty(JKeys.WaterBoiler.PDStopHour)]
		public int StopHour { get; set; }
		[JsonProperty(JKeys.WaterBoiler.PDStopMin)]
		public int StopMinute { get; set; }
	}
}
