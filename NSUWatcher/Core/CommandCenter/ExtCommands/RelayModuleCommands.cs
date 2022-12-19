using NSU.Shared;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.ExtCommands;

namespace NSUWatcher.CommandCenter.ExtCommands
{
    public class RelayModuleCommands : IRelayModuleCommands
    {
        private readonly INsuSerializer _serializer;

        public RelayModuleCommands(INsuSerializer serializer)
        {
            _serializer = serializer;
        }

        public IExternalCommand Setup(byte configPos, bool enabled, bool activeLow, bool reversed)
        {
            return new ExternalCommand()
            {
                Target = JKeys.RelayModule.TargetName,
                Action = JKeys.Generic.Setup,
                Content = _serializer.Serialize( new RelayModuleSetupContent(configPos, enabled, activeLow, reversed) )
            };
        }

        public IExternalCommand OpenChannel(byte channel)
        {
            return new ExternalCommand()
            {
                Target = JKeys.RelayModule.TargetName,
                Action = JKeys.RelayModule.ActionOpenChannel,
                Content = _serializer.Serialize( new RelayModuleOpenChContent(true, channel) )
            };
        }

        public IExternalCommand CloseChannel(byte channel)
        {
            return new ExternalCommand()
            {
                Target = JKeys.RelayModule.TargetName,
                Action = JKeys.RelayModule.ActionCloseChannel,
                Content = _serializer.Serialize( new RelayModuleOpenChContent(false, channel) )
            };
        }

        public IExternalCommand LockChannel(byte channel, bool openOnLock)
        {
            return new ExternalCommand()
            {
                Target = JKeys.RelayModule.TargetName,
                Action = JKeys.RelayModule.ActionLockChannel,
                Content = _serializer.Serialize( new RelayModuleLockChContent(true, channel, openOnLock) )
            };
        }

        public IExternalCommand UnlockChannel(byte channel)
        {
            return new ExternalCommand()
            {
                Target = JKeys.RelayModule.TargetName,
                Action = JKeys.RelayModule.ActionUnlockChannel,
                Content = _serializer.Serialize( new RelayModuleLockChContent(false, channel, false) )
            };
        }
    }
}
