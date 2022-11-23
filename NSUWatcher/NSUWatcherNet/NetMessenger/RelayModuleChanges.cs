using NSU.Shared.DataContracts;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUSystemPart;

namespace NSUWatcher.NSUWatcherNet.NetMessenger
{
    public static class RelayModuleChanges
    {
        public static NetMessage? Message(IRelayModuleDataContract dataContract, string property)
        {
            return property switch
            {
                nameof(RelayModule.StatusFlags) => new NetMessage(
                    RelayModuleChanged.Create(dataContract.ConfigPos, dataContract.StatusFlags, dataContract.LockFlags)
                    ),
                _ => null
            };
        }
    }
}
