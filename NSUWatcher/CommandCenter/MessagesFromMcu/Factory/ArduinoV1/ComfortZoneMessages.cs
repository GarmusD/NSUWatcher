using Newtonsoft.Json.Linq;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data;
using NSU.Shared;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1
{
    public class ComfortZoneMessages : IFromArduinoV1Base
    {
        public IMessageFromMcu? TryFindMessage(JObject command)
        {
            string target = (string)command[JKeys.Generic.Target]!;
            if (target != JKeys.ComfortZone.TargetName) return null;

            string action = (string)command[JKeys.Generic.Action]!;

            return action switch
            {
                JKeys.Action.Snapshot => command.ToObject<ComfortZoneSnapshot>(),
                JKeys.Action.Info => GetInfoByContent(command),
                _ => null
            };
        }

        private static IMessageFromMcu? GetInfoByContent(JObject command)
        {
            string content = (string)command[JKeys.Generic.Content]!;
            return content switch
            {
                JKeys.ComfortZone.CurrentRoomTemp => command.ToObject<ComfortZoneRoomTempInfo>(),
                JKeys.ComfortZone.CurrentFloorTemp => command.ToObject<ComfortZoneFloorTempInfo>(),
                JKeys.ComfortZone.ActuatorOpened => command.ToObject<ComfortZoneActuatorStatus>(),
                JKeys.ComfortZone.LowTempMode => command.ToObject<ComfortZoneLowTempMode>(),
                _ => null
            };
        }
    }
}
