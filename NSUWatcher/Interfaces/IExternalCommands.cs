using NSUWatcher.Interfaces.ExtCommands;

namespace NSUWatcher.Interfaces
{
    public interface IExternalCommands
    {
        public IBinUploaderCommands BinUploaderCommands { get; }
        public IConsoleCommands ConsoleCommands { get; }
        public IUserCmdCommands UserCmdCommands { get; }
        public ICircPumpCommands CircPumpCommands { get; }
        public ICollectorCommands CollectorCommands { get; }
        public IComfortZoneCommands ComfortZoneCommands { get; }
        public IKTypeCommands KTypeCommands { get; }
        public IRelayModuleCommands RelayModuleCommands { get; }
        public ISystemCommands SystemCommands { get; }
        public ISystemFanCommands SystemFanCommands { get; }
        public ISwitchCommands SwitchCommands { get; }
        public ITempSensorCommands TempSensorCommands { get; }
        public ITempTriggerCommands TempTriggerCommands { get; }
        public IWaterBoilerCommands WaterBoilerCommands { get; }
        public IWoodBoilerCommands WoodBoilerCommands { get; }
    }
}
