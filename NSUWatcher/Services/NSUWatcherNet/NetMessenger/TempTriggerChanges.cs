using NSU.Shared.DataContracts;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUSystemPart;

namespace NSUWatcher.Services.NSUWatcherNet.NetMessenger
{
    public static class TempTriggerChanges
    {
        public static NetMessage Message(ITempTriggerDataContract dataContract, string property)
        {
            switch (property)
            {
                case nameof(TempTrigger.Status):
                    return new NetMessage(
                        TempTriggerStatusChanged.Create(dataContract.Name, dataContract.Status.ToString())
                    );
                default: return null;
            };
        }
    }
}
