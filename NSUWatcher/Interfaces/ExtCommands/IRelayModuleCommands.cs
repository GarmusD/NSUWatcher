using NSU.Shared.DTO;

namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface IRelayModuleCommands
    {
        public IExternalCommand Setup(byte configPos, bool enabled, bool activeLow, bool reversed);
        public IExternalCommand OpenChannel(byte channel);
        public IExternalCommand CloseChannel(byte channel);
        public IExternalCommand LockChannel(byte channel, bool openOnLock);
        public IExternalCommand UnlockChannel(byte channel);
    }
}
