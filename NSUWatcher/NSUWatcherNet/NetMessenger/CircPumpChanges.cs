using NSU.Shared.DataContracts;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUSystemPart;

namespace NSUWatcher.NSUWatcherNet.NetMessenger
{
    public static class CircPumpChanges
    {
        public static NetMessage Message(ICircPumpDataContract dataContract, string property)
        {
            switch (property)
            {
                case nameof(CircPump.Status): return new NetMessage(
                    CircPumpStatusChanged.Create(dataContract.Name, dataContract.Status.ToString(), dataContract.CurrentSpeed.ToString(), dataContract.OpenedValvesCount.ToString())
                    );
                default: return null;
            };
        }
    }
}
