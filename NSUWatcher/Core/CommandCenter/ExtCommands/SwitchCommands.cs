using NSU.Shared.NSUSystemPart;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.ExtCommands;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSU.Shared;

namespace NSUWatcher.CommandCenter.ExtCommands
{
    public class SwitchCommands : ISwitchCommands
    {
        private readonly INsuSerializer _serializer;

        public SwitchCommands(INsuSerializer serializer)
        {
            _serializer = serializer;
        }

        public IExternalCommand Click(string name)
        {
            return new ExternalCommand() 
            {
                Target = JKeys.Switch.TargetName,
                Action = JKeys.Action.Click,
                Content = _serializer.Serialize( new SwitchClickContent(name) )
            };
        }

        public IExternalCommand Setup(byte configPos, bool enabled, string name, string depName, Status onDepStatus, Status forceStatus, Status defaultStatus)
        {
            return new ExternalCommand()
            {
                Target = JKeys.Switch.TargetName,
                Action = JKeys.Generic.Setup,
                Content = _serializer.Serialize( new SwitchSetupContent(configPos, enabled, name, depName, onDepStatus, forceStatus, defaultStatus) )
            };
        }
    }
}
