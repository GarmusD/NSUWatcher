using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class CollectorInfo : ICollectorInfo
    {
        public string Name { get; set; } = string.Empty;
        public bool[] OpenedValves { get; set; } = new bool[0];
        public string Target { get; set; } = string.Empty;
        public string? CommandID { get; set; }
    }
}
