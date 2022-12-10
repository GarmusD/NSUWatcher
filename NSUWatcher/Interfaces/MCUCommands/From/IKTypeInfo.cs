namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IKTypeInfo : IMessageFromMcu
    {
        string Name { get; set; }
        double Value { get; set; }
    }
}
