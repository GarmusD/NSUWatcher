namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ISwitchInfo : IMessageFromMcu
    {
        string Name { get; set; }
        string Status { get; set; }
        bool IsForced { get; set; }
    }
}
