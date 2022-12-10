namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IComfortZoneActuatorStatus : IMessageFromMcu
    {
        string Name { get; set; }
        bool Value { get; set; }
    }
}
