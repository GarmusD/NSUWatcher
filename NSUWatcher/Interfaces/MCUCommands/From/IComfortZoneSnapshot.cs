using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IComfortZoneSnapshot : IMessageFromMcu
    {
        [JsonProperty(JKeys.Generic.ConfigPos)]
        public int ConfigPos { get; set; }
        [JsonProperty(JKeys.Generic.Enabled)]
        public bool Enabled { get; set; }
        [JsonProperty(JKeys.Generic.Name)]
        public string Name { get; set; }
        [JsonProperty(JKeys.ComfortZone.Title)]
        public string Title { get; set; }
        [JsonProperty(JKeys.ComfortZone.CollectorName)]
        public string CollectorName { get; set; }
        [JsonProperty(JKeys.ComfortZone.Actuator)]
        public int ActuatorPosition { get; set; }
        [JsonProperty(JKeys.ComfortZone.Histeresis)]
        public double Histeresis { get; set; }
        [JsonProperty(JKeys.ComfortZone.RoomSensorName)]
        public string RoomSensorName { get; set; }
        [JsonProperty(JKeys.ComfortZone.RoomTempHi)]
        public double RoomTempHi { get; set; }
        [JsonProperty(JKeys.ComfortZone.RoomTempLow)]
        public double RoomTempLow { get; set; }
        [JsonProperty(JKeys.ComfortZone.FloorSensorName)]
        public string FloorSensorName { get; set; }
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
    }
}
