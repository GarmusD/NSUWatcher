using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    internal class SystemSetTimeResult : ISystemSetTimeResult
    {
        [JsonProperty(JKeys.Generic.Result)]
        public string Result { get; set; }
    }
}
