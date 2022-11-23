using NSU.Shared;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.ExtCommands;

namespace NSUWatcher.CommandCenter.ExtCommands
{
    public class SystemCommands : ISystemCommands
    {
        private readonly INsuSerializer _serializer;

        public SystemCommands(INsuSerializer serializer)
        {
            _serializer = serializer;
        }

        public IExternalCommand ResetMcu(ResetType resetType)
        {
            return new ExternalCommand() 
            {
                Target = JKeys.Syscmd.TargetName,
                Action = JKeys.Syscmd.RebootSystem,
                Content = _serializer.Serialize( new SysResetContent(resetType) )
            };
        }
    }
}
