using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data
{
    public class ComfortZoneSnapshot : IComfortZoneSnapshot
    {
        public int ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string CollectorName { get; set; } = string.Empty;
        public int ActuatorPosition { get; set; }
        public double Histeresis { get; set; }
        public string RoomSensorName { get; set; } = string.Empty;
        public double RoomTempHi { get; set; }
        public double RoomTempLow { get; set; }
        public string FloorSensorName { get; set; } = string.Empty;
        public double FloorTempHi { get; set; }
        public double FloorTempLow { get; set; }
        public bool LowTempMode { get; set; }
        public double? CurrentRoomTemp { get; set; }
        public double? CurrentFloorTemp { get; set; }
        public bool? ActuatorOpened { get; set; }
        public string Target { get; set; } = string.Empty;
        public string? CommandID { get; set; }
    }
}
