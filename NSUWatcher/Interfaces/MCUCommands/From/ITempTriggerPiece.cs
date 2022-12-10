using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ITempTriggerPiece : IMessageFromMcu
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
