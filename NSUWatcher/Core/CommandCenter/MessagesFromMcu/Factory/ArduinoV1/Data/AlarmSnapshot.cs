using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
#nullable enable
    public class AlarmSnapshot : IAlarmSnapshot
    {
        [JsonProperty(JKeys.Generic.ConfigPos)]
        public int ConfgPos { get; set; }
        
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

        [JsonProperty(JKeys.Generic.CommandID)]
        public string? CommandID { get; set; }
    }

    public class AlarmChannel : IAlarmChannel
    {
        [JsonProperty(JKeys.Alarm.Channel)]
        public int Channel { get; set; }

        [JsonProperty(JKeys.Alarm.IsOpen)]
        public bool IsOpen { get; set; }
    }
#nullable disable
}
