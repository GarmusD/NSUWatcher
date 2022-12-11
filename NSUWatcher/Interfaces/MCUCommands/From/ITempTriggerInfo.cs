namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ITempTriggerInfo : IMessageFromMcu
    {
        string Name { get; set; }
        string Status { get; set; }
    }
}
