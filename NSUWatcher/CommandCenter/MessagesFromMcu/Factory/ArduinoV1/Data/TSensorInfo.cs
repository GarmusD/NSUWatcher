using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class TSensorInfo : ITSensorInfo
    {
        public string SensorID { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public int ReadErrorCount { get; set; }
        public string Target { get; set; } = string.Empty;
        public string? CommandID { get; set; }
    }
}
