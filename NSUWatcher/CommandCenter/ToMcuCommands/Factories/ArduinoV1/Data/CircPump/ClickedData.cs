using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands;

namespace NSUWatcher.CommandCenter.ToMcuCommands.Factories.ArduinoV1.Data.CircPump
{
    public struct ClickedData : ICommandToMcuData
    {
        [JsonProperty(JKeys.Generic.Target)]
        public string Target { get; }
        [JsonProperty(JKeys.Generic.Action)]
        public string Action { get; }
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; }

        internal ClickedData(string name)
        {
            Target = JKeys.CircPump.TargetName;
            Action = JKeys.CircPump.ActionClick;
            Name = name;
        }
    }
}
