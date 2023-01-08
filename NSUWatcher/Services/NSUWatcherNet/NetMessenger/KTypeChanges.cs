using NSU.Shared.DataContracts;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUSystemPart;

namespace NSUWatcher.Services.NSUWatcherNet.NetMessenger
{
#nullable enable
    public static class KTypeChanges
    {
        public static NetMessage? Message(IKTypeDataContract dataContract, string property)
        {
            return property switch
            {
                nameof(KType.Temperature) => new NetMessage(
                                    KTypeTempChanged.Create(dataContract.Name, dataContract.Temperature)
                                    ),
                _ => null,
            };
            ;
        }
    }
}
