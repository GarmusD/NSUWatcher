﻿using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class SystemFanInfo : ISystemFanInfo
    {
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Target { get; set; } = string.Empty;
        public string? CommandID { get; set; }
    }
}
