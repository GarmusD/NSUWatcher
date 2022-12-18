using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands;

namespace NSUWatcher.CommandCenter.ToMcuCommands.Factories.ArduinoV1.Data.CircPump
{
#nullable enable
    public readonly struct ClickedData : ICommandToMcuData
    {
        [JsonProperty(JKeys.Generic.Target)]
        public string Target { get; }
        [JsonProperty(JKeys.Generic.Action)]
        public string Action { get; }
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; }

        [JsonProperty(JKeys.Generic.CommandID)]
        public string? CommandId { get; }

        internal ClickedData(string name, string? commandId = null)
        {
            Target = JKeys.CircPump.TargetName;
            Action = JKeys.CircPump.ActionClick;
            CommandId = commandId;
            Name = name;
        }
    }
#nullable disable
}
