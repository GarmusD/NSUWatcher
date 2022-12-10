using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
#nullable enable
    public class ComfortZoneSnapshot : IComfortZoneSnapshot
    {
        [JsonProperty(JKeys.Generic.ConfigPos)]
        public byte ConfigPos { get; set; }
        
        [JsonProperty(JKeys.Generic.Enabled)]
        public bool Enabled { get; set; }
        
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.ComfortZone.Title)]
        public string Title { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.ComfortZone.CollectorName)]
        public string CollectorName { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.ComfortZone.Actuator)]
        public int ActuatorPosition { get; set; }
        
        [JsonProperty(JKeys.ComfortZone.Histeresis)]
        public double Histeresis { get; set; }
        
        [JsonProperty(JKeys.ComfortZone.RoomSensorName)]
        public string RoomSensorName { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.ComfortZone.RoomTempHi)]
        public double RoomTempHi { get; set; }
        
        [JsonProperty(JKeys.ComfortZone.RoomTempLow)]
        public double RoomTempLow { get; set; }
        
        [JsonProperty(JKeys.ComfortZone.FloorSensorName)]
        public string FloorSensorName { get; set; } = string.Empty;
        
        [JsonProperty(JKeys.ComfortZone.FloorTempHi)]
        public double FloorTempHi { get; set; }
        
        [JsonProperty(JKeys.ComfortZone.FloorTempLow)]
        public double FloorTempLow { get; set; }
        
        [JsonProperty(JKeys.ComfortZone.LowTempMode)]
        public bool LowTempMode { get; set; }
        
        [JsonProperty(JKeys.ComfortZone.CurrentRoomTemp)]
        public double? CurrentRoomTemp { get; set; }
        
        [JsonProperty(JKeys.ComfortZone.CurrentFloorTemp)]
        public double? CurrentFloorTemp { get; set; }
        
        [JsonProperty(JKeys.ComfortZone.ActuatorOpened)]
        public bool? ActuatorOpened { get; set; }
        
        //[JsonProperty(JKeys.Generic.Source)]
        //public string Source { get; set; } = string.Empty;
        
        //[JsonProperty(JKeys.Generic.CommandID)]
        //public string? CommandID { get; set; }
    }
#nullable disable
}
