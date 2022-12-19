using NSU.Shared;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.ExtCommands;

namespace NSUWatcher.CommandCenter.ExtCommands
{
    public class KTypeCommands : IKTypeCommands
    {
        private readonly INsuSerializer _serializer;

        public KTypeCommands(INsuSerializer serializer)
        {
            _serializer = serializer;
        }

        public IExternalCommand Setup(byte configPos, bool enabled, string name, int interval)
        {
            return new ExternalCommand()
            {
                Target = JKeys.KType.TargetName,
                Action = JKeys.Generic.Setup,
                Content = _serializer.Serialize( new KTypeSetupContent(configPos, enabled, name, interval) )
            };
        }
    }
}
