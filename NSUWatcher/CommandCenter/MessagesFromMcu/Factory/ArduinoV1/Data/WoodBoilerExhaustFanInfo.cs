using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class WoodBoilerExhaustFanInfo : IWoodBoilerExhaustFanInfo
    {
        public string Name { get; set; } = string.Empty;
        public string ExhaustFanStatus { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public string? CommandID { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    }
}
