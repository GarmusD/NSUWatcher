using NSU.Shared.DataContracts;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUSystemPart;

namespace NSUWatcher.NSUWatcherNet.NetMessenger
{
    public static class SwitchChanges
    {
        public static NetMessage Message(ISwitchDataContract dataContract, string property)
        {
            switch (property)
            {
                case nameof(Switch.Status): return new NetMessage(
                    SwitchStatusChanged.Create(dataContract.Name, dataContract.Status.ToString(), dataContract.IsForced)
                    );
                default: return null;
            };
        }
    }
}
