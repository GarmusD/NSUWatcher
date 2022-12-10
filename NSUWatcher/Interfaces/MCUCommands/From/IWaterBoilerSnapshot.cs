namespace NSUWatcher.Interfaces.MCUCommands.From
{
	public interface IWaterBoilerSnapshot : IMessageFromMcu
    {
		byte ConfigPos { get; set; }
		bool Enabled { get; set; }
		string Name { get; set; }
		string TempSensorName { get; set; }
		string CircPumpName { get; set; }
		string TempTriggerName { get; set; }
		bool ElHeatingEnabled { get; set; }
		int ElPowerChannel { get; set; }	
		IElHeatingDataSnapshot[] ElHeatingData { get; set; }
	}
	
	public interface IElHeatingDataSnapshot
	{
		int StartHour { get; set; }
		int StartMinute { get; set; }
		int StopHour { get; set; }
		int StopMinute { get; set; }
	}
}
