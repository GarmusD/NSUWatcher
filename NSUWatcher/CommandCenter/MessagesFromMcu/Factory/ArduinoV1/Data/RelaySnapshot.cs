using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class RelaySnapshot : IRelaySnapshot
    {
        public byte ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public bool ActiveLow { get; set; }
        public bool Reversed { get; set; }
        public byte? StatusFlags { get; set; }
        public byte? LockFlags { get; set; }
        public string Target { get; set; } = string.Empty;
        public string? CommandID { get; set; }
    }
}
