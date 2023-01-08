using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands;

namespace NSUWatcher.Core.CommandCenter.ToMcuCommands.Factories.ArduinoV1.Data.WoodBoiler
{
#nullable enable
    public readonly struct ActionIkurimasData : ICommandToMcuData
    {
        [JsonProperty(JKeys.Generic.Target)]
        public string Target { get; }
        [JsonProperty(JKeys.Generic.Action)]
        public string Action { get; }
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; }
        [JsonProperty(JKeys.Generic.Value)]
        public string? CommandId { get; }

        public ActionIkurimasData(string wbName, string? commandId = null)
        {
            Target = JKeys.WoodBoiler.TargetName;
            Action = JKeys.WoodBoiler.ActionIkurimas;
            Name = wbName;
            CommandId = commandId;
        }
    }
}
