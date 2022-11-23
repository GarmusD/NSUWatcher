using NSU.Shared.DataContracts;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUSystemPart;

namespace NSUWatcher.NSUWatcherNet.NetMessenger
{
    public static class ComfortZoneChanges
    {
        public static NetMessage? Message(IComfortZoneDataContract dataContract, string property)
        {
            return property switch
            {
                nameof(ComfortZone.CurrentRoomTemperature) => new NetMessage(
                    ComfortZoneRoomTempChanged.Create(dataContract.Name, dataContract.CurrentRoomTemperature)
                    ),
                nameof(ComfortZone.CurrentFloorTemperature) => new NetMessage(
                   ComfortZoneFloorTempChanged.Create(dataContract.Name, dataContract.CurrentRoomTemperature)
                   ),
                nameof(ComfortZone.ActuatorOpened) => new NetMessage(
                   ComfortZoneActuatorOpenedChanged.Create(dataContract.Name, dataContract.ActuatorOpened)
                   ),
                _ => null
            };
        }
    }
}
