namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IRelayChannelClosed : IMessageFromMcu
    {
        byte Channel { get; set; }
        bool IsLocked { get; set; }
    }
}
