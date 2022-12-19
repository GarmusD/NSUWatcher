using NSU.Shared.DataContracts;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUNet;

namespace NSUWatcher.NSUWatcherNet.NetMessenger
{
    public static class AlarmChanges
    {
        public static NetMessage Message(IAlarmDataContract alarmData, string property)
        {
            return new NetMessage(new AlarmStatusChanged());
        }
    }
}
