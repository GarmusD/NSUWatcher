using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class TSensorConfigSnapshot : ITSensorConfigSnapshot
    {
        public int ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Interval { get; set; }
        public string Target { get; set; } = string.Empty;
        public string? CommandID { get; set; }
    }
}
