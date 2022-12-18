namespace NSUWatcher.Interfaces.MCUCommands.From
{
	public interface IAlarmSnapshot : IMessageFromMcu
    {
		int ConfgPos { get; set; }
		bool Enabled { get; set; }
		double AlarmTemperature { get; set; }
		double Histeresis { get; set; }
		IAlarmChannel[] ChannelData { get; set; }
		bool? IsAlarming { get; set; }
    }

    public interface IAlarmChannel
    {
        int Channel { get; set; }
        bool IsOpen { get; set; }
    }
}
