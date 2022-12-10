using NSU.Shared.DTO.ExtCommandContent;

namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface ITempTriggerCommands
    {
        public IExternalCommand Setup(byte configPos, bool enabled, string name, params TriggerPiece[] triggerPieces);
    }
}
