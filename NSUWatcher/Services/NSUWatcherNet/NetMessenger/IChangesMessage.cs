using NSU.Shared.DataContracts;
using NSU.Shared.NSUNet;

namespace NSUWatcher.Services.NSUWatcherNet.NetMessenger
{
    public interface IChangesMessage
    {
        INetMessage Message(INSUSysPartDataContract dataContract, string property);
    }
}
