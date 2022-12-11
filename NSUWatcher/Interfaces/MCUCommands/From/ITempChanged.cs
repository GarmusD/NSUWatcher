namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ITempChanged : IMessageFromMcu
    {
        string Address { get; set; }
        float Temperature { get; set; }
        int ReadErrorCount { get; set; }
    }
}
