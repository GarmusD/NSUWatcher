using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IAlarmChannel : IMessageFromMcu
	{
		[JsonProperty(JKeys.Alarm.Channel)]
		public int Channel { get; set; }
		[JsonProperty(JKeys.Alarm.IsOpen)]
		public bool IsOpen { get; set; }
	}
}
