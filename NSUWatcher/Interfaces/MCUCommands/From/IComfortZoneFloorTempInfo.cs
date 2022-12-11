namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IComfortZoneFloorTempInfo : IMessageFromMcu
    {
        public string Name { get; set; }
        double Value { get; set; }
    }
}
