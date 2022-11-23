﻿using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands;

namespace NSUWatcher.CommandCenter.ToMcuCommands.Factories.ArduinoV1.Data.TempSensor
{
    public struct SetupTempSensorData : ICommandToMcuData
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

        public string Target { get; }

        public string Action { get; }

        public SetupTempSensorData(int configPos, bool enabled, string name, string address, int interval)
        {
            Target = JKeys.TempSensor.TargetName;
            Action = JKeys.Generic.Setup;
            ConfigPos = configPos;
            Interval = interval;
            Enabled = enabled;
            Name = name;
            Address = address;
        }
    }
}
