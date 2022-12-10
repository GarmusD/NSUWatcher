using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ICircPumpSnapshot : IMessageFromMcu
    {
        [JsonProperty(JKeys.Generic.ConfigPos)]
        public int ConfigPos { get; set; }
        [JsonProperty(JKeys.Generic.Enabled)]
        public bool Enabled { get; set; }
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; set; }
        [JsonProperty(JKeys.CircPump.MaxSpeed)]
        public int MaxSpeed { get; set; }
        [JsonProperty(JKeys.CircPump.Speed1Ch)]
        public int Speed1Ch { get; set; }
        [JsonProperty(JKeys.CircPump.Speed2Ch)]
        public int Speed2Ch { get; set; }
        [JsonProperty(JKeys.CircPump.Speed3Ch)]
        public int Speed3Ch { get; set; }
        [JsonProperty(JKeys.CircPump.TempTriggerName)]
        public string TempTriggerName { get; set; }
        [JsonProperty(JKeys.Generic.Status)]
        public string? Status { get; set; }
        [JsonProperty(JKeys.CircPump.CurrentSpeed)]
        public int? CurrentSpeed { get; set; }
        [JsonProperty(JKeys.CircPump.ValvesOpened)]
        public int? OpenedValvesCount { get; set; }
    }
}
