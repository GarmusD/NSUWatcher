using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class WoodBoilerLadomatInfo : IWoodBoilerLadomatInfo
    {
        public string Name { get; set; } = string.Empty;
        public string LadomatStatus { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public string? CommandID { get; set; }
    }
}
