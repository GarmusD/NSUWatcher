using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class CircPumpSnapshot : ICircPumpSnapshot
    {
        public int ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MaxSpeed { get; set; }
        public int Speed1Ch { get; set; }
        public int Speed2Ch { get; set; }
        public int Speed3Ch { get; set; }
        public string TempTriggerName { get; set; } = string.Empty;
        public string? Status { get; set; }
        public int? CurrentSpeed { get; set; }
        public int? OpenedValvesCount { get; set; }
        public string Target { get; set; } = string.Empty;
        public string? CommandID { get; set; }
    }
}
