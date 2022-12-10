﻿using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ISystemFanInfo : IMessageFromMcu
    {
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; set; }
        [JsonProperty(JKeys.Generic.Value)]
        double Value { get; set; }
    }
}