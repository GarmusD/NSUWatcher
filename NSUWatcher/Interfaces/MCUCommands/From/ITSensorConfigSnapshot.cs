namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ITSensorConfigSnapshot : IMessageFromMcu
    {
        byte ConfigPos { get; set; }
        bool Enabled { get; set; }
        string Address { get; set; }
        string Name { get; set; }
        int Interval { get; set; }
    }
}
