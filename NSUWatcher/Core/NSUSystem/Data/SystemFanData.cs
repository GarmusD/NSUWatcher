using NSU.Shared.DataContracts;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.NSUSystem.Data
{
    public class SystemFanData : ISystemFanDataContract
    {
        public byte ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TempSensorName { get; set; } = string.Empty;
        public double MinTemp { get; set; }
        public double MaxTemp { get; set; }
        public int CurrentPWM { get; set; }

        public SystemFanData()
        {
        }

        public SystemFanData(ISystemFanSnapshot snapshot)
        {
            ConfigPos = snapshot.ConfigPos;
            Enabled = snapshot.Enabled;
            Name = snapshot.Name;
            TempSensorName = snapshot.TempSensorName;
            MinTemp = snapshot.MinTemperature;
            MaxTemp = snapshot.MaxTemperature;
            CurrentPWM = snapshot.CurrentPWM.GetValueOrDefault();
        }
    }
}
