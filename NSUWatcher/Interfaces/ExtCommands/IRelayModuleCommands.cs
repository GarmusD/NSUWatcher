using NSU.Shared.DTO;

namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface IRelayModuleCommands
    {
        IExternalCommand Setup(byte configPos, bool enabled, bool activeLow, bool reversed);
        IExternalCommand OpenChannel(byte channel);
        IExternalCommand CloseChannel(byte channel);
        IExternalCommand LockChannel(byte channel, bool openOnLock);
        IExternalCommand UnlockChannel(byte channel);
    }
}
