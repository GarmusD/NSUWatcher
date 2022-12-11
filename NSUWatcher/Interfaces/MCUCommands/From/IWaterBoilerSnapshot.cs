namespace NSUWatcher.Interfaces.MCUCommands.From
{

	public interface IWaterBoilerSnapshot : IMessageFromMcu
    {
		public byte ConfigPos { get; set; }
		public bool Enabled { get; set; }
		public string Name { get; set; }
		public string TempSensorName { get; set; }
		public string CircPumpName { get; set; }
		public string TempTriggerName { get; set; }
		public bool ElHeatingEnabled { get; set; }
		public byte ElPowerChannel { get; set; }
		public IElHeatingDataSnapshot[] ElHeatingData { get; set; }
	}
	
	public interface IElHeatingDataSnapshot
	{
		public byte StartHour { get; set; }
		public byte StartMinute { get; set; }
		public byte StopHour { get; set; }
		public byte StopMinute { get; set; }
	}
}
