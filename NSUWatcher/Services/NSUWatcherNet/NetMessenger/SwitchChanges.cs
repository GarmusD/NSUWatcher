using NSU.Shared.DataContracts;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUSystemPart;

namespace NSUWatcher.NSUWatcherNet.NetMessenger
{
#nullable enable
    public static class SwitchChanges
    {
        public static NetMessage? Message(ISwitchDataContract dataContract, string property)
        {
            return property switch
            {
                nameof(Switch.Status) => new NetMessage(
                                    SwitchStatusChanged.Create(dataContract.Name, dataContract.Status.ToString(), dataContract.IsForced)
                                    ),
                _ => null,
            };
        }
    }
}
