namespace NSUWatcher.Interfaces.MCUCommands.From
{
#nullable enable
	public interface IWoodBoilerSnapshot : IMessageFromMcu
    {
		byte ConfigPos { get; set; }
		bool Enabled { get; set; }
		string Name { get; set; }
		double WorkingTemperature { get; set; }
		double Histeresis { get; set; }
		string TempSensorName { get; set; }
		string KTypeName { get; set; }
		int ExhaustFanChannel { get; set; }
		int LadomChannel { get; set; }
		string LadomatTriggerName { get; set; }
		double LadomatWorkingTemp { get; set; }
		string WaterBoilerName { get; set; }
		double? CurrentTemperature { get; set; }
		string? Status { get; set; }
		string? LadomatStatus { get; set; }
		string? ExhaustFanStatus { get; set; }
		string? TemperatureStatus { get; set; }
    }
#nullable disable
}
