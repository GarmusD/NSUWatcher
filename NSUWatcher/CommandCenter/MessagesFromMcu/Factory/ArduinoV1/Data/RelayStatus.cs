using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class RelayStatus : IRelayInfo
    {
        public IRelayModuleStatus[] Values { get; set; } = new IRelayModuleStatus[0];
        public string Target { get; set; } = string.Empty;
        public string? CommandID { get; set; }
    }

    public class RelayModuleStatus : IRelayModuleStatus
    {
        public byte StatusFlags { get; set; }
        public byte LockFlags { get; set; }
    }
}
