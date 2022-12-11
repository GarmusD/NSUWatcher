namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IRelaySnapshot : IMessageFromMcu
    {
        byte ConfigPos { get; set; }
        bool Enabled { get; set; }
        bool ActiveLow { get; set; }
        bool Reversed { get; set; }
        byte? StatusFlags { get; set; }
        byte? LockFlags { get; set; }
    }
}
