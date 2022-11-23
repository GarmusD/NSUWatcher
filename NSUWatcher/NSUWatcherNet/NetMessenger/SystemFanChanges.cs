using NSU.Shared.DataContracts;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUSystemPart;

namespace NSUWatcher.NSUWatcherNet.NetMessenger
{
    public static class SystemFanChanges
    {
        public static NetMessage? Message(ISystemFanDataContract dataContract, string property)
        {
            return property switch
            {
                nameof(SystemFan.CurrentPWM) => new NetMessage(
                    SystemFanPwmChanged.Create(dataContract.Name, dataContract.CurrentPWM)
                    ),
                _ => null
            };
        }
    }
}
