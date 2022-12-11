namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IComfortZoneRoomTempInfo : IMessageFromMcu
    {
        public string Name { get; set; }
        double Value { get; set; }
    }
}
