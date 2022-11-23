using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class TSensorSystemSnapshot : ITSensorSystemSnapshot
    {
        public string Address { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public int ReadErrors { get; set; }
        public string Target { get; set; } = string.Empty;
        public string? CommandID { get; set; }
    }
}
