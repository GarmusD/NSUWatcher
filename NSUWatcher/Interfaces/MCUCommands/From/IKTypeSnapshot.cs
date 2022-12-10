namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IKTypeSnapshot : IMessageFromMcu
    {
        byte ConfigPos { get; set; }
        bool Enabled { get; set; }
        string Name { get; set; }
        int Interval { get; set; }
        int? Temperature { get; set; }
    }
}
