namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IComfortZoneActuatorStatus : IMessageFromMcu
    {
        public string Name { get; set; }
        bool Value { get; set; }
    }
}
