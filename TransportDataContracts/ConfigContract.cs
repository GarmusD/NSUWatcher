using System;
using System.Collections.Generic;
using System.Text;

namespace TransportDataContracts
{
    public class ConfigContract
    {
        public string ComPort { get; set; } = string.Empty;
        public int BaudRate { get; set; } = 0;
        public RebootMcuConfig RebootMcu { get; set; } = new RebootMcuConfig();
    }

    public class RebootMcuConfig
    {
        public int BaudRate { get; set; } = 0;
        public bool DtrPulseOnly { get; set; } = false;
        public int Delay { get; set; }
        public int ReconnectDelay { get; set; }
    }
}
