using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ITempTriggerSnapshot : IMessageFromMcu
    {
        [JsonProperty(JKeys.Generic.ConfigPos)]
        public int ConfigPos { get; set; }
        [JsonProperty(JKeys.Generic.Enabled)]
        public bool Enabled { get; set; }
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; set; }
        [JsonProperty(JKeys.TempTrigger.Pieces)]
        public ITempTriggerPiece[] TempTriggerPieces { get; set; }
#nullable enable
        [JsonProperty(JKeys.Generic.Status)]
        public string? Status { get; set; }
#nullable disable
    }
}
