using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands;

namespace NSUWatcher.CommandCenter.ToMcuCommands.Factories.ArduinoV1.Data.System
{
#nullable enable
    public readonly struct SetTimeData : ICommandToMcuData
    {
        [JsonProperty(JKeys.Generic.Target)]
        public string Target { get; }
        [JsonProperty(JKeys.Generic.Action)]
        public string Action { get; }
        [JsonProperty(JKeys.Syscmd.Year)]
        public int Year { get; }
        [JsonProperty(JKeys.Syscmd.Month)]
        public int Month { get; }
        [JsonProperty(JKeys.Syscmd.Day)]
        public int Day { get; }
        [JsonProperty(JKeys.Syscmd.Hour)]
        public int Hour { get; }
        [JsonProperty(JKeys.Syscmd.Minute)]
        public int Minute { get; }
        [JsonProperty(JKeys.Syscmd.Second)]
        public int Second { get; }

        [JsonProperty(JKeys.Generic.CommandID)]
        public string? CommandId { get; }

        public SetTimeData(int year, int month, int day, int hourt, int minute, int second, string? commandId)
        {
            Target = JKeys.Syscmd.TargetName;
            Action = JKeys.Syscmd.SetTime;
            Year = year;
            Month = month;
            Day = day;
            Hour = hourt;
            Minute = minute;
            Second = second;
            CommandId = commandId;
        }
    }
#nullable disable
}
