using NSU.Shared.DataContracts;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUSystemPart;

namespace NSUWatcher.Services.NSUWatcherNet.NetMessenger
{
    public static class CircPumpChanges
    {
        public static NetMessage Message(ICircPumpDataContract dataContract, string property)
        {
            return property switch
            {
                nameof(CircPump.Status) => new NetMessage(
                                    CircPumpStatusChanged.Create(dataContract.Name, dataContract.Status.ToString(), dataContract.CurrentSpeed.ToString(), dataContract.OpenedValvesCount.ToString())
                                    ),
                _ => null,
            };
            ;
        }
    }
}
