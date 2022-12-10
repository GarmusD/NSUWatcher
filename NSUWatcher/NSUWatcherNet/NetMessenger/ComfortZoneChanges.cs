using NSU.Shared.DataContracts;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUSystemPart;

namespace NSUWatcher.NSUWatcherNet.NetMessenger
{
    public static class ComfortZoneChanges
    {
        public static NetMessage Message(IComfortZoneDataContract dataContract, string property)
        {
            switch (property)
            {
                case nameof(ComfortZone.CurrentRoomTemperature): return new NetMessage(
                    ComfortZoneRoomTempChanged.Create(dataContract.Name, dataContract.CurrentRoomTemperature)
                    );
                case nameof(ComfortZone.CurrentFloorTemperature): return new NetMessage(
                   ComfortZoneFloorTempChanged.Create(dataContract.Name, dataContract.CurrentRoomTemperature)
                   );
                case nameof(ComfortZone.ActuatorOpened): return new NetMessage(
                   ComfortZoneActuatorOpenedChanged.Create(dataContract.Name, dataContract.ActuatorOpened)
                   );
                default: return null;
            };
        }
    }
}
