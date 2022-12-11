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
		byte StartHour { get; set; }
		byte StartMinute { get; set; }
		byte StopHour { get; set; }
		byte StopMinute { get; set; }
	}
}
