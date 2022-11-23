using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class WoodBoilerSnapshot : IWoodBoilerSnapshot
    {
        public int ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; } = string.Empty;
        public double WorkingTemperature { get; set; }
        public double Histeresis { get; set; }
        public string TempSensorName { get; set; } = string.Empty;
        public string KTypeName { get; set; } = string.Empty;
        public int ExhaustFanChannel { get; set; }
        public int LadomChannel { get; set; }
        public string LadomatTriggerName { get; set; } = string.Empty;
        public double LadomatWorkingTemp { get; set; }
        public string WaterBoilerName { get; set; } = string.Empty;
        public double? CurrentTemperature { get; set; }
        public string? Status { get; set; }
        public string? LadomatStatus { get; set; }
        public string? ExhaustFanStatus { get; set; }
        public string? TemperatureStatus { get; set; }
        public string Target { get; set; } = string.Empty;
        public string? CommandID { get; set; }
    }
}
