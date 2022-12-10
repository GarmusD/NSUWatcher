using NSU.Shared.DTO;

namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface IKTypeCommands
    {
        public IExternalCommand Setup(byte configPos, bool enabled, string name, int interval);
    }
}
