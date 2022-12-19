using Newtonsoft.Json.Linq;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1.Data;
using NSU.Shared;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1
{
    public class ComfortZoneMessages : IFromArduinoV1Base
    {
        public IMessageFromMcu TryFindMessage(JObject command)
        {
            string source = (string)command[JKeys.Generic.Source];
            if (source != JKeys.ComfortZone.TargetName) return null;

            string action = (string)command[JKeys.Generic.Action];

            switch (action)
            {
                case JKeys.Action.Snapshot: return command.ToObject<ComfortZoneSnapshot>();
                case JKeys.Action.Info: return GetInfoByContent(command);
                default: return null;
            };
        }

        private static IMessageFromMcu GetInfoByContent(JObject command)
        {
            string content = (string)command[JKeys.Generic.Content];
            switch (content)
            {
                case JKeys.ComfortZone.CurrentRoomTemp: return command.ToObject<ComfortZoneRoomTempInfo>();
                case JKeys.ComfortZone.CurrentFloorTemp: return command.ToObject<ComfortZoneFloorTempInfo>();
                case JKeys.ComfortZone.ActuatorOpened: return command.ToObject<ComfortZoneActuatorStatus>();
                case JKeys.ComfortZone.LowTempMode: return command.ToObject<ComfortZoneLowTempMode>();
                default: return null;
            };
        }
    }
}
