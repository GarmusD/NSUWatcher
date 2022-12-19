using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.ExtCommands;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSU.Shared;

namespace NSUWatcher.CommandCenter.ExtCommands
{
    public class TempTriggerCommands : ITempTriggerCommands
    {
        private readonly INsuSerializer _serializer;

        public TempTriggerCommands(INsuSerializer serializer)
        {
            _serializer = serializer;
        }

        public IExternalCommand Setup(byte configPos, bool enabled, string name, params TriggerPiece[] triggerPieces)
        {
            return new ExternalCommand()
            {
                Target = JKeys.TempTrigger.TargetName,
                Action = JKeys.Generic.Setup,
                Content = _serializer.Serialize( new TempTriggerSetupContent(configPos, enabled, name, triggerPieces) )
            };
        }
    }
}
