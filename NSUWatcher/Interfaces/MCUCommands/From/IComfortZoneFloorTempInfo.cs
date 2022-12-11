namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IComfortZoneFloorTempInfo : IMessageFromMcu
    {
        string Name { get; set; }
        double Value { get; set; }
    }
}
