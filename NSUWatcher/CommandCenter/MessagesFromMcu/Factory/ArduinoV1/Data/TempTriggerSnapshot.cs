using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
#nullable enable
    public class TempTriggerSnapshot : ITempTriggerSnapshot
    {
        [JsonProperty(JKeys.Generic.ConfigPos)]
        public byte ConfigPos { get; set; }
                
        [JsonProperty(JKeys.Generic.Enabled)]
        public bool Enabled { get; set; }

        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.TempTrigger.Pieces)]
        public ITempTriggerPiece[] TempTriggerPieces { get; set; } = new TempTriggerPiece[0];
        
        [JsonProperty(JKeys.Generic.Status)]
        public string? Status { get; set; }

        [JsonProperty(JKeys.Generic.CommandID)]
        public string? CommandID { get; set; }
    }
#nullable disable

    public class TempTriggerPiece : ITempTriggerPiece
    {
        [JsonProperty(JKeys.Generic.Enabled)]
        public bool Enabled { get; set; }
        
        [JsonProperty(JKeys.TempTrigger.TempSensorName)]
        public string TSensorName { get; set; }
        
        [JsonProperty(JKeys.TempTrigger.TriggerCondition)]
        public int Condition { get; set; }
        
        [JsonProperty(JKeys.TempTrigger.TriggerTemperature)]
        public double Temperature { get; set; }
        
        [JsonProperty(JKeys.TempTrigger.TriggerHisteresis)]
        public double Histeresis { get; set; }
    }
}
