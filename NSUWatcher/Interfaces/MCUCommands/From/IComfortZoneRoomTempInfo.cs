namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IComfortZoneRoomTempInfo : IMessageFromMcu
    {
        string Name { get; set; }
        double Value { get; set; }
    }
}
