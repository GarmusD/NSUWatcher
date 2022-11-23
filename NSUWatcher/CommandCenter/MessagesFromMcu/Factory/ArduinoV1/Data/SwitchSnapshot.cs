using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class SwitchSnapshot : ISwitchSnapshot
    {
        public int ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DependOnName { get; set; } = string.Empty;
        public string DependancyStatus { get; set; } = string.Empty;
        public string ForceState { get; set; } = string.Empty;
        public bool? IsForced { get; set; }
        public string? CurrentState { get; set; }
        public string Target { get; set; } = string.Empty;
        public string? CommandID { get; set; }
    }
}
