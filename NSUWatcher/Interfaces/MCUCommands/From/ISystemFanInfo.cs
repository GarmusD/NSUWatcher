namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ISystemFanInfo : IMessageFromMcu
    {
        string Name { get; set; }
        double Value { get; set; }
    }
}
