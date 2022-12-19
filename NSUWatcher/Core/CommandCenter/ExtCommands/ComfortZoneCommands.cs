using NSU.Shared;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.ExtCommands;

namespace NSUWatcher.CommandCenter.ExtCommands
{
    public class ComfortZoneCommands : IComfortZoneCommands
    {
        private readonly INsuSerializer _serializer;

        public ComfortZoneCommands(INsuSerializer serializer)
        {
            _serializer = serializer;
        }

        public IExternalCommand Setup(  byte configPos, bool enabled, string name, string title, string roomTempSensor, string floorTempSensor, 
                                        string collector, double roomTempHi, double roomTempLo, double floorTempHi, double floorTempLo, 
                                        double histerezis, byte actuator, bool lowTempMode)
        {
            return new ExternalCommand()
            {
                Target = JKeys.ComfortZone.TargetName,
                Action = JKeys.Generic.Setup,
                Content = _serializer.Serialize(
                    new ComfortZoneSetupContent(
                        configPos, enabled, name, title, roomTempSensor, floorTempSensor, collector, roomTempHi,
                        roomTempLo, floorTempHi, floorTempLo, histerezis, actuator, lowTempMode
                    )
                )
            };
        }

        public IExternalCommand Update(byte configPos, double? roomTempHi, double? roomTempLo, double? floorTemoHi, double? floorTempLo, bool? lowTempMode)
        {
            return new ExternalCommand()
            {
                Target = JKeys.ComfortZone.TargetName,
                Action = JKeys.Action.Update,
                Content = _serializer.Serialize( new ComfortZoneUpdateContent(configPos, roomTempHi, roomTempLo, floorTemoHi, floorTempLo, lowTempMode) )
            };
        }
    }
}
