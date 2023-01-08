using NSU.Shared.DataContracts;
using NSU.Shared.DTO.NsuNet;

namespace NSUWatcher.Services.NSUWatcherNet.NetMessenger
{
#nullable enable
    public static class AlarmChanges
    {
        public static NetMessage? Message(IAlarmDataContract alarmData, string property)
        {
            return new NetMessage(new AlarmStatusChanged());
        }
    }
}
