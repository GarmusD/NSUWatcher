namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IComfortZoneLowTempMode : IMessageFromMcu
    {
        string Name { get; set; }
        bool Value { get; set; }
    }
}
