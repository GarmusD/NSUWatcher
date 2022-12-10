using NSU.Shared.DataContracts;
using NSU.Shared.NSUNet;

namespace NSUWatcher.NSUWatcherNet.NetMessenger
{
    public interface IChangesMessage
    {
        public INetMessage Message(INSUSysPartDataContract dataContract, string property);
    }
}
