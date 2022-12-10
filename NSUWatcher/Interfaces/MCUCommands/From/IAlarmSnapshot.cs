using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
	public interface IAlarmSnapshot : IMessageFromMcu
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
		public IAlarmChannel[] ChannelData { get; set; }
		[JsonProperty(JKeys.Generic.Value)]
		public bool? IsAlarming { get; set; }
    }
}
