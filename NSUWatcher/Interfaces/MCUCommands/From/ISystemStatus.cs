using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ISystemStatus : IMessageFromMcu
    {
        [JsonProperty(propertyName: JKeys.Generic.Value)]
        public string CurrentState { get; set; }
        [JsonProperty(propertyName: JKeys.Syscmd.FreeMem)]
        public int FreeMem { get; set; }
        [JsonProperty(propertyName: JKeys.Syscmd.UpTime, Required = Required.AllowNull)]
        public int? UpTime { get; set; }
        [JsonProperty(JKeys.Syscmd.RebootRequired)]
        public bool RebootRequired { get; set; }
    }
}
