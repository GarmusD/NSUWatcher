using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class AlarmSnapshot : IAlarmSnapshot
    {
        [JsonProperty(JKeys.Generic.ConfigPos)]
        public byte ConfigPos { get; set; }
        
        [JsonProperty(JKeys.Generic.Enabled)]
        public bool Enabled { get; set; }
        
        [JsonProperty(JKeys.Alarm.Temp)]
        public double AlarmTemperature { get; set; }
        
        [JsonProperty(JKeys.Alarm.Histeresis)]
        public double Histeresis { get; set; }
        
        [JsonProperty(JKeys.Alarm.ChannelData)]
        public IAlarmChannel[] ChannelData { get; set; } = new AlarmChannel[0];
        
        [JsonProperty(JKeys.Generic.Value)]
        public bool? IsAlarming { get; set; }
    }

    public class AlarmChannel : IAlarmChannel
    {
        [JsonProperty(JKeys.Alarm.Channel)]
        public byte Channel { get; set; }

        [JsonProperty(JKeys.Alarm.IsOpen)]
        public bool IsOpen { get; set; }
    }
}
