namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IComfortZoneSnapshot : IMessageFromMcu
    {
        public byte ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string CollectorName { get; set; }
        public int ActuatorPosition { get; set; }
        public double Histeresis { get; set; }
        public string RoomSensorName { get; set; }
        public double RoomTempHi { get; set; }
        public double RoomTempLow { get; set; }
        public string FloorSensorName { get; set; }
        public double FloorTempHi { get; set; }
        public double FloorTempLow { get; set; }
        public bool LowTempMode { get; set; }
        public double? CurrentRoomTemp { get; set; }
        public double? CurrentFloorTemp { get; set; }
        public bool? ActuatorOpened { get; set; }
    }
}
