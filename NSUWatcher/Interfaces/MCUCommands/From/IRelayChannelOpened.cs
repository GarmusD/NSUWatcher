namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IRelayChannelOpened : IMessageFromMcu
    {
        byte Channel { get; set; }
        bool IsLocked { get; set; }
    }
}
