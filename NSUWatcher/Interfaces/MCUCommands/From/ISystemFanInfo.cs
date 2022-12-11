namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ISystemFanInfo : IMessageFromMcu
    {
        public string Name { get; set; }
        double Value { get; set; }
    }
}
