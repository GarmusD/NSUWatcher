using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class SystemStatus : ISystemStatus
    {
        public string CurrentState { get; set; } = string.Empty;
        public int FreeMem { get; set; }
        public int? UpTime { get; set; }
        public bool RebootRequired { get; set; }
        public string Target { get; set; } = string.Empty;
        public string? CommandID { get; set; }
    }
}
