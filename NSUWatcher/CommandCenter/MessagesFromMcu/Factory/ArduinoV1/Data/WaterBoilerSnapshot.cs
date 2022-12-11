using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
#nullable enable
    public class WaterBoilerSnapshot : IWaterBoilerSnapshot
    {
        [JsonProperty(JKeys.Generic.ConfigPos)]
        public byte ConfigPos { get; set; }
        
        [JsonProperty(JKeys.Generic.Enabled)]
        public bool Enabled { get; set; }
        
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.WaterBoiler.TempSensorName)]
        public string TempSensorName { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.WaterBoiler.CircPumpName)]
        public string CircPumpName { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.WaterBoiler.TempTriggerName)]
        public string TempTriggerName { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.WaterBoiler.ElHeatingEnabled)]
        public bool ElHeatingEnabled { get; set; }
        
        [JsonProperty(JKeys.WaterBoiler.ElPowerChannel)]
        public byte ElPowerChannel { get; set; }
        
        [JsonProperty(JKeys.WaterBoiler.PowerData)]
        public IElHeatingDataSnapshot[] ElHeatingData { get; set; } = new HeatingData[0];
    }
#nullable disable

    public class HeatingData : IElHeatingDataSnapshot
    {
        [JsonProperty(JKeys.WaterBoiler.PDStartHour)]
        public byte StartHour { get; set; }
        
        [JsonProperty(JKeys.WaterBoiler.PDStartMin)]
        public byte StartMinute { get; set; }
        
        [JsonProperty(JKeys.WaterBoiler.PDStopHour)]
        public byte StopHour { get; set; }
        
        [JsonProperty(JKeys.WaterBoiler.PDStopMin)]
        public byte StopMinute { get; set; }
    }
}
