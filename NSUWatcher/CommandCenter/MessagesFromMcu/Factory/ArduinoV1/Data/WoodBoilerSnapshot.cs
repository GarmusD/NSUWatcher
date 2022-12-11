using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
#nullable enable
    public class WoodBoilerSnapshot : IWoodBoilerSnapshot
    {
        [JsonProperty(JKeys.Generic.ConfigPos)]
        public byte ConfigPos { get; set; }
        
        [JsonProperty(JKeys.Generic.Enabled)]
        public bool Enabled { get; set; }
        
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.WoodBoiler.WorkingTemp)]
        public double WorkingTemperature { get; set; }
        
        [JsonProperty(JKeys.WoodBoiler.Histeresis)]
        public double Histeresis { get; set; }
        
        [JsonProperty(JKeys.WoodBoiler.TSensorName)]
        public string TempSensorName { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.WoodBoiler.KTypeName)]
        public string KTypeName { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.WoodBoiler.ExhaustFanChannel)]
        public int ExhaustFanChannel { get; set; }
        
        [JsonProperty(JKeys.WoodBoiler.LadomChannel)]
        public int LadomChannel { get; set; }
        
        [JsonProperty(JKeys.WoodBoiler.LadomatTriggerName)]
        public string LadomatTriggerName { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.WoodBoiler.LadomatWorkTemp)]
        public double LadomatWorkingTemp { get; set; }
        
        [JsonProperty(JKeys.WoodBoiler.WaterBoilerName)]
        public string WaterBoilerName { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.WoodBoiler.CurrentTemp)]
        public double? CurrentTemperature { get; set; }
        
        [JsonProperty(JKeys.Generic.Status, Required = Required.AllowNull)]
        public string? Status { get; set; }
        
        [JsonProperty(JKeys.WoodBoiler.LadomatStatus, Required = Required.AllowNull)]
        public string? LadomatStatus { get; set; }
        
        [JsonProperty(JKeys.WoodBoiler.ExhaustFanStatus, Required = Required.AllowNull)]
        public string? ExhaustFanStatus { get; set; }
        
        [JsonProperty(JKeys.WoodBoiler.TemperatureStatus, Required = Required.AllowNull)]
        public string? TemperatureStatus { get; set; }
        
        //[JsonProperty(JKeys.Generic.Source)]
        //public string Source { get; set; } = string.Empty;
        
        //[JsonProperty(JKeys.Generic.CommandID)]
        //public string? CommandID { get; set; }
    }
#nullable enable
}
