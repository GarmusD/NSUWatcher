using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class CollectorSnapshot : ICollectorSnapshot
    {
        public int ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CircPumpName { get; set; } = string.Empty;
        public int ActuatorsCounts { get; set; }
        public ICollectorActuator[] Actuators { get; set; } = new CollectorActuator[0];
        public string Target { get; set; } = string.Empty;
        public string? CommandID { get; set; }
    }

    public class CollectorActuator : ICollectorActuator
    {
        public int ActuatorType { get; set; }
        public int Channel { get; set; }
        public bool? IsOpen { get; set; }
    }
}
