namespace NSUWatcher.Interfaces.MCUCommands.From
{
	public interface ISystemFanSnapshot : IMessageFromMcu
    {
		byte ConfigPos { get; set; }
		bool Enabled { get; set; }
		string Name { get; set; }
		string TempSensorName { get; set; }
		double MinTemperature { get; set; }
		double MaxTemperature { get; set; }
		int? CurrentPWM { get; set; }
    }
}
