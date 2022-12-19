using NSU.Shared.DataContracts;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUSystemPart;

namespace NSUWatcher.NSUWatcherNet.NetMessenger
{
    public static class KTypeChanges
    {
        public static NetMessage Message(IKTypeDataContract dataContract, string property)
        {
            switch (property)
            {
                case nameof(KType.Temperature): return new NetMessage(
                    KTypeTempChanged.Create(dataContract.Name, dataContract.Temperature)
                    );
                default: return null;
            };
        }
    }
}
