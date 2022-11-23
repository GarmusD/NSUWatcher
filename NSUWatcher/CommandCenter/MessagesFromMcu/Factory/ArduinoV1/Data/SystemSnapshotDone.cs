using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class SystemSnapshotDone : ISystemSnapshotDone
    {
        public string Target { get; set; } = string.Empty;
        public string? CommandID { get; set; }
    }
}
