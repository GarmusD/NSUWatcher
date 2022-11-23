using NSU.Shared.Serializer;
using NSUWatcher.CommandCenter.ExtCommands;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.ExtCommands;

namespace NSUWatcher.CommandCenter
{
    public class ExternalCommands : IExternalCommands
    {
        public IBinUploaderCommands BinUploaderCommands => _binUploaderCommands;
        public IConsoleCommands ConsoleCommands => _consoleCommands;
        public IUserCmdCommands UserCmdCommands => _userCmdCommands;
        public ICircPumpCommands CircPumpCommands => _circPumpCommands;
        public ICollectorCommands CollectorCommands => _collectorCommands;
        public IComfortZoneCommands ComfortZoneCommands => _comfortZoneCommands;
        public IKTypeCommands KTypeCommands => _kTypeCommands;
        public IRelayModuleCommands RelayModuleCommands => _relayModuleCommands;
        public ISystemCommands SystemCommands => _systemCommands;
        public ISystemFanCommands SystemFanCommands => _systemFanCommands;
        public ISwitchCommands SwitchCommands => _switchCommands;
        public ITempSensorCommands TempSensorCommands => _tempSensorCommands;
        public ITempTriggerCommands TempTriggerCommands => _tempTriggerCommands;
        public IWaterBoilerCommands WaterBoilerCommands => _waterBoilerCommands;
        public IWoodBoilerCommands WoodBoilerCommands => _woodBoilerCommands;

        private readonly IBinUploaderCommands _binUploaderCommands;
        private readonly ICircPumpCommands _circPumpCommands;
        private readonly ICollectorCommands _collectorCommands;
        private readonly IComfortZoneCommands _comfortZoneCommands;
        private readonly IKTypeCommands _kTypeCommands;
        private readonly IRelayModuleCommands _relayModuleCommands;
        private readonly ISystemFanCommands _systemFanCommands;
        private readonly ISwitchCommands _switchCommands;
        private readonly ISystemCommands _systemCommands;
        private readonly ITempSensorCommands _tempSensorCommands;
        private readonly ITempTriggerCommands _tempTriggerCommands;
        private readonly IWaterBoilerCommands _waterBoilerCommands;
        private readonly IWoodBoilerCommands _woodBoilerCommands;
        private readonly IConsoleCommands _consoleCommands;
        private readonly IUserCmdCommands _userCmdCommands;

        public ExternalCommands(INsuSerializer serializer)
        {
            _binUploaderCommands = new BinUploaderCommands(serializer);
            _consoleCommands = new ConsoleCommands(serializer);
            _circPumpCommands = new CircPumpCommands(serializer);
            _collectorCommands = new CollectorCommands(serializer);
            _comfortZoneCommands = new ComfortZoneCommands(serializer);
            _kTypeCommands = new KTypeCommands(serializer);
            _relayModuleCommands = new RelayModuleCommands(serializer);
            _systemFanCommands = new SystemFanCommands(serializer);
            _switchCommands = new SwitchCommands(serializer);
            _systemCommands = new SystemCommands(serializer);
            _tempSensorCommands = new TempSensorCommands(serializer);
            _tempTriggerCommands = new TempTriggerCommands(serializer);
            _waterBoilerCommands = new WaterBoilerCommands(serializer);
            _woodBoilerCommands = new WoodBoilerCommands(serializer);
            _userCmdCommands = new UserCmdCommands(serializer);
        }
    }
}
