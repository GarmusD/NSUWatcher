namespace NSUWatcher.Interfaces.MCUCommands.From
{
	public interface IElHeatingData
	{
		public byte StartHour { get; set; }
		public byte StartMinute { get; set; }
		public byte StopHour { get; set; }
		public byte StopMinute { get; set; }
	}
}
