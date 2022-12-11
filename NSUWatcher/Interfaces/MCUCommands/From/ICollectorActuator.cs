namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ICollectorActuator
    {
        public int ActuatorType { get; set; }
        public byte Channel { get; set; }
        public bool? IsOpen { get; set; }
    }
}
