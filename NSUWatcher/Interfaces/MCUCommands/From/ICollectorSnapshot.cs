namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ICollectorSnapshot : IMessageFromMcu
    {
        byte ConfigPos { get; set; }
        bool Enabled { get; set; }
        string Name { get; set; }
        string CircPumpName { get; set; }
        int ActuatorsCounts { get; set; }
        ICollectorActuator[] Actuators { get; set; }
    }

    public interface ICollectorActuator
    {
        int ActuatorType { get; set; }
        byte Channel { get; set; }
        bool? IsOpen { get; set; }
    }
}
