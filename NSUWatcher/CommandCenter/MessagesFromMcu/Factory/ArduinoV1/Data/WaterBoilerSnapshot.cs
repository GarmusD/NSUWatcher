using NSU.Shared.DataContracts;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class WaterBoilerSnapshot : IWaterBoilerSnapshot
    {
        public int ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TempSensorName { get; set; } = string.Empty;
        public string CircPumpName { get; set; } = string.Empty;
        public string TempTriggerName { get; set; } = string.Empty;
        public bool ElHeatingEnabled { get; set; }
        public int ElPowerChannel { get; set; }
        public IElHeatingDataSnapshot[] ElHeatingData { get; set; } = new HeatingData[0];
        public string Target { get; set; } = string.Empty;
        public string? CommandID { get; set; }
    }

    public class HeatingData : IElHeatingDataSnapshot
    {
        public int StartHour { get; set; }
        public int StartMinute { get; set; }
        public int StopHour { get; set; }
        public int StopMinute { get; set; }
    }
}
