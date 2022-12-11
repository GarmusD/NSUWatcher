namespace NSUWatcher.Interfaces.MCUCommands.From
{
	public interface IWoodBoilerSnapshot : IMessageFromMcu
    {
		public byte ConfigPos { get; set; }
		public bool Enabled { get; set; }
		public string Name { get; set; }
		public double WorkingTemperature { get; set; }
		public double Histeresis { get; set; }
		public string TempSensorName { get; set; }
		public string KTypeName { get; set; }
		public int ExhaustFanChannel { get; set; }
		public int LadomChannel { get; set; }
		public string LadomatTriggerName { get; set; }
		public double LadomatWorkingTemp { get; set; }
		public string WaterBoilerName { get; set; }
		public double? CurrentTemperature { get; set; }
		public string? Status { get; set; }
		public string? LadomatStatus { get; set; }
		public string? ExhaustFanStatus { get; set; }
		public string? TemperatureStatus { get; set; }
    }
}
