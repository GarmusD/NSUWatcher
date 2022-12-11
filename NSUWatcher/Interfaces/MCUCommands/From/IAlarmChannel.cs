namespace NSUWatcher.Interfaces.MCUCommands.From
{
	public interface IAlarmChannel : IMessageFromMcu
	{
		int Channel { get; set; }
		bool IsOpen { get; set; }
	}
}
