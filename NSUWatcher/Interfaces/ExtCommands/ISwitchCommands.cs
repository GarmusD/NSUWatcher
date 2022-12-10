using NSU.Shared.NSUSystemPart;

namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface ISwitchCommands
    {
        IExternalCommand Setup(byte configPos, bool enabled, string name, string depName, Status onDepStatus, Status forceStatus, Status defaultStatus);
        IExternalCommand Click(string name);
    }
}
