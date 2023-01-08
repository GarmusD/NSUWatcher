using NSU.Shared.DataContracts;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUSystemPart;

namespace NSUWatcher.Services.NSUWatcherNet.NetMessenger
{
    public static class RelayModuleChanges
    {
        public static NetMessage Message(IRelayModuleDataContract dataContract, string property)
        {
            switch (property)
            {
                case nameof(RelayModule.StatusFlags): return new NetMessage(
                    RelayModuleChanged.Create(dataContract.ConfigPos, dataContract.StatusFlags, dataContract.LockFlags)
                    );
                default: return null;
            };
        }
    }
}
