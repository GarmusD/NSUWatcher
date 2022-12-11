using NSU.Shared.DTO.ExtCommandContent;

namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface IComfortZoneCommands
    {
        IExternalCommand Setup(byte configPos, bool enabled, string name, string title, string roomTempSensor, string floorTempSensor, string collector,
            double roomTempHi, double roomTempLo, double floorTempHi, double floorTempLo, double histerezis, byte actuator, bool lowTempMode);
        IExternalCommand Update(byte configPos, double? roomTempHi, double? roomTempLo, double? floorTemoHi, double? floorTempLo, bool? lowTempMode);
    }
}
