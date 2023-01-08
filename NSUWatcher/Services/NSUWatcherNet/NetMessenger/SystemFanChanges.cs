using NSU.Shared.DataContracts;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUSystemPart;

namespace NSUWatcher.Services.NSUWatcherNet.NetMessenger
{
    public static class SystemFanChanges
    {
        public static NetMessage Message(ISystemFanDataContract dataContract, string property)
        {
            switch (property)
            {
                case nameof(SystemFan.CurrentPWM): 
                    return new NetMessage(
                        SystemFanPwmChanged.Create(dataContract.Name, dataContract.CurrentPWM)
                    );
                default: return null;
            };
        }
    }
}
