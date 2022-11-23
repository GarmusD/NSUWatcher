using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IElHeatingData
	{
		[JsonProperty(JKeys.WaterBoiler.PDStartHour)]
		public int StartHour { get; set; }
		[JsonProperty(JKeys.WaterBoiler.PDStartMin)]
		public int StartMinute { get; set; }
		[JsonProperty(JKeys.WaterBoiler.PDStopHour)]
		public int StopHour { get; set; }
		[JsonProperty(JKeys.WaterBoiler.PDStopMin)]
		public int StopMinute { get; set; }
	}
}
