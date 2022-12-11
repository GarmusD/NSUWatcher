namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IComfortZoneSnapshot : IMessageFromMcu
    {
        byte ConfigPos { get; set; }
        bool Enabled { get; set; }
        string Name { get; set; }
        string Title { get; set; }
        string CollectorName { get; set; }
        int ActuatorPosition { get; set; }
        double Histeresis { get; set; }        
        string RoomSensorName { get; set; }
        double RoomTempHi { get; set; }
        double RoomTempLow { get; set; }
        string FloorSensorName { get; set; }
        double FloorTempHi { get; set; }
        double FloorTempLow { get; set; }
        bool LowTempMode { get; set; }
        double? CurrentRoomTemp { get; set; }
        double? CurrentFloorTemp { get; set; }
        bool? ActuatorOpened { get; set; }
    }
}
