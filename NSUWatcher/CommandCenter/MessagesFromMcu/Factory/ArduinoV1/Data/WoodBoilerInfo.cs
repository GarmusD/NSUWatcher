using NSU.Shared.NSUSystemPart;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class WoodBoilerInfo : IWoodBoilerInfo
    {
        public string Name { get; set; } = string.Empty;
        public double CurrentTemperature { get; set; }
        public string WBStatus { get; set; } = string.Empty;
        public string TempStatus { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public string? CommandID { get; set; }
    }
}
