using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class RelayChannelClosed : IRelayChannelClosed
    {
        [JsonProperty(JKeys.Generic.Value)]
        public byte Channel { get; set; }
        
        [JsonProperty(JKeys.RelayModule.IsLocked)]
        public bool IsLocked { get; set; }
    }
}
