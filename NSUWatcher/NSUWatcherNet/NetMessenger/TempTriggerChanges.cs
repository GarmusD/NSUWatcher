using NSU.Shared.DataContracts;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUSystemPart;

namespace NSUWatcher.NSUWatcherNet.NetMessenger
{
    public static class TempTriggerChanges
    {
        public static NetMessage? Message(ITempTriggerDataContract dataContract, string property)
        {
            return property switch
            {
                nameof(TempTrigger.Status) => new NetMessage(
                    TempTriggerStatusChanged.Create(dataContract.Name, dataContract.Status.ToString())
                    ),
                _ => null
            };
        }
    }
}
