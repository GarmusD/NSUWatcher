namespace NSUWatcher.Interfaces.MCUCommands.From
{
	public interface ICollectorInfo : IMessageFromMcu
    {
		string Name { get; set; }
		bool[] OpenedValves { get; set; }
	}
}
