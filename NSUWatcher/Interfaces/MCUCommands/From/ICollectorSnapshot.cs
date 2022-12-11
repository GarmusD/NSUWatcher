namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ICollectorSnapshot : IMessageFromMcu
    {
        public byte ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public string CircPumpName { get; set; }
        public int ActuatorsCounts { get; set; }
        public ICollectorActuator[] Actuators { get; set; }
    }
}
