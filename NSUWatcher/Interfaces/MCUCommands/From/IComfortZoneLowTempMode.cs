namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IComfortZoneLowTempMode : IMessageFromMcu
    {
        public string Name { get; set; }
        bool Value { get; set; }
    }
}
