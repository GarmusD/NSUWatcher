using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands;

namespace NSUWatcher.Core.CommandCenter.ToMcuCommands.Factories.ArduinoV1.Data.WoodBoiler
{
#nullable enable
    public readonly struct SwitchManualModeData : ICommandToMcuData
    {
        [JsonProperty(JKeys.Generic.Target)]
        public string Target { get; }
        [JsonProperty(JKeys.Generic.Action)]
        public string Action { get; }
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; }
        [JsonProperty(JKeys.Generic.Value)]
        public string Value { get; }
        [JsonProperty(JKeys.Generic.CommandID)]
        public string? CommandId { get; }

        public SwitchManualModeData(string wbName, string switchTarget, string? commandId = null)
        {
            Target = JKeys.WoodBoiler.TargetName;
            Action = JKeys.WoodBoiler.ActionSwitch;
            Name = wbName;
            Value = switchTarget;
            CommandId = commandId;
        }
    }
}
