using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands;

namespace NSUWatcher.CommandCenter.ToMcuCommands.Factories.ArduinoV1.Data.System
{
#nullable enable
    public readonly struct EmptyCommand : ICommandToMcuData
    {
        [JsonProperty(JKeys.Generic.Target)]
        public string Target => JKeys.Syscmd.TargetName;

        [JsonProperty(JKeys.Generic.Action)]
        public string Action { get; }

        [JsonProperty(JKeys.Generic.CommandID)]
        public string? CommandId => null;

        public EmptyCommand(string action = "no_action")
        {
            Action = action;
        }
    }
#nullable disable
}
