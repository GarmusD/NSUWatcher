using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class KTypeSnapshot : IKTypeSnapshot
    {
        public int ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Interval { get; set; }
        public int? Temperature { get; set; }
        public string Target { get; set; } = string.Empty;
        public string? CommandID { get; set; }
    }
}
