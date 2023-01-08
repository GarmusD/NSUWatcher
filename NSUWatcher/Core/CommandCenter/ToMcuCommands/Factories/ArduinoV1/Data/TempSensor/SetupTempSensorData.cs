using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands;

namespace NSUWatcher.Core.CommandCenter.ToMcuCommands.Factories.ArduinoV1.Data.TempSensor
{
#nullable enable
    public readonly struct SetupTempSensorData : ICommandToMcuData
    {
        [JsonProperty(JKeys.Generic.Enabled)]
        public bool Enabled { get; }
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; }
        [JsonProperty(JKeys.TempSensor.SensorID)]
        public string Address { get; }
        [JsonProperty(JKeys.TempSensor.Interval)]
        public int Interval { get; }
        [JsonProperty(JKeys.Generic.ConfigPos)]
        public int ConfigPos { get; }
        [JsonProperty(JKeys.Generic.Target)]
        public string Target { get; }
        [JsonProperty(JKeys.Generic.Action)]
        public string Action { get; }
        [JsonProperty(JKeys.Generic.CommandID)]
        public string? CommandId { get; }

        public SetupTempSensorData(int configPos, bool enabled, string name, string address, int interval, string? commandId)
        {
            Target = JKeys.TempSensor.TargetName;
            Action = JKeys.Generic.Setup;
            ConfigPos = configPos;
            Interval = interval;
            Enabled = enabled;
            Name = name;
            Address = address;
            CommandId = commandId;
        }
    }
#nullable disable
}
