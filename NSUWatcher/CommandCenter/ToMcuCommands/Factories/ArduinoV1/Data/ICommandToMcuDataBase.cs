using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.CommandCenter.ToMcuCommands.Factories.ArduinoV1.Data
{
    public interface ICommandToMcuDataBase
    {
        [JsonProperty(JKeys.Generic.Target)]
        public string Target { get; set; }
        [JsonProperty(JKeys.Generic.Action)]
        public string Action { get; set; }
    }
}
