using NSU.Shared.DataContracts;
using NSU.Shared.NSUSystemPart;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.NSUSystem.Data
{
    public class WaterBoilerDataContract : IWaterBoilerDataContract
    {
        public byte ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TempSensorName { get; set; } = string.Empty;
        public string TempTriggerName { get; set; } = string.Empty;
        public string CircPumpName { get; set; } = string.Empty;
        public bool ElHeatingEnabled { get; set; }
        public int ElHeatingChannel { get; set; }
        public IElHeatingDataDataContract[] ElHeatingData { get; set; } = new ElHeatingDataContract[WaterBoiler.MAX_WATERBOILER_EL_HEATING_COUNT];

        public WaterBoilerDataContract()
        {

        }

        public WaterBoilerDataContract(IWaterBoilerSnapshot snapshot)
        {
            ConfigPos = snapshot.ConfigPos;
            Enabled = snapshot.Enabled;
            Name = snapshot.Name;
            TempSensorName = snapshot.TempSensorName;
            TempTriggerName = snapshot.TempTriggerName;
            CircPumpName = snapshot.CircPumpName;
            ElHeatingEnabled = snapshot.ElHeatingEnabled;
            ElHeatingChannel = snapshot.ElPowerChannel;
            for (int i = 0; i < WaterBoiler.MAX_WATERBOILER_EL_HEATING_COUNT; i++)
            {
                ElHeatingData[i] = new ElHeatingDataContract() 
                { 
                    Index = (byte)i,
                    StartHour = snapshot.ElHeatingData[i].StartHour,
                    StartMin = snapshot.ElHeatingData[i].StartMinute,
                    EndHour = snapshot.ElHeatingData[i].StopHour,
                    EndMin = snapshot.ElHeatingData[i].StopMinute
                };
            }
    }
    }

    public class ElHeatingDataContract : IElHeatingDataDataContract
    {
        public byte Index { get; set; }
        public int StartHour { get; set; }
        public int StartMin { get; set; }
        public int EndHour { get; set; }
        public int EndMin { get; set; }
    }
}
