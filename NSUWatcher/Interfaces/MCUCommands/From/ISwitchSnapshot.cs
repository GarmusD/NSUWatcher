using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ISwitchSnapshot : IMessageFromMcu
    {
        [JsonProperty(JKeys.Generic.ConfigPos)]
        public int ConfigPos { get; set; }
        [JsonProperty(JKeys.Generic.Enabled)]
        public bool Enabled { get; set; }
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; set; }
        [JsonProperty(JKeys.Switch.Dependancy)]
        public string DependOnName { get; set; }
        [JsonProperty(JKeys.Switch.OnDependancyStatus)]
        public string DependancyStatus { get; set; }
        [JsonProperty(JKeys.Switch.ForceStatus)]
        public string ForceState { get; set; }
        [JsonProperty(JKeys.Switch.IsForced)]
        public bool? IsForced { get; set; }
        [JsonProperty(JKeys.Switch.CurrState)]
        public string? CurrentState { get; set; }
    }
}
