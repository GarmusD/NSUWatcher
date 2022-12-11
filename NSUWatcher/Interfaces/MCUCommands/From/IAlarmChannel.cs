namespace NSUWatcher.Interfaces.MCUCommands.From
{
	public interface IAlarmChannel : IMessageFromMcu
	{
		public byte Channel { get; set; }
		public bool IsOpen { get; set; }
	}
}
