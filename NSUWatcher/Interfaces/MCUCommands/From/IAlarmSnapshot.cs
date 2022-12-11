namespace NSUWatcher.Interfaces.MCUCommands.From
{
	public interface IAlarmSnapshot : IMessageFromMcu
    {
		public byte ConfigPos { get; set; }
		public bool Enabled { get; set; }
		public double AlarmTemperature { get; set; }
		public double Histeresis { get; set; }
		public IAlarmChannel[] ChannelData { get; set; }
		public bool? IsAlarming { get; set; }
    }
}
