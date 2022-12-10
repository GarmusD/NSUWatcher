using NSU.Shared.DTO.ExtCommandContent;

namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface ISystemCommands
    {
        public IExternalCommand ResetMcu(ResetType resetType);
    }
}
