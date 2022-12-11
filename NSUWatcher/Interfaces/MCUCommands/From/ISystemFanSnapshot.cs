namespace NSUWatcher.Interfaces.MCUCommands.From
{
	public interface ISystemFanSnapshot : IMessageFromMcu
    {
		public byte ConfigPos { get; set; }
		public bool Enabled { get; set; }
		public string Name { get; set; }
		public string TempSensorName { get; set; }
		public double MinTemperature { get; set; }
		public double MaxTemperature { get; set; }
		public int? CurrentPWM { get; set; }
    }
}
