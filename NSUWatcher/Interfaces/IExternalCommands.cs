using NSUWatcher.Interfaces.ExtCommands;

namespace NSUWatcher.Interfaces
{
    public interface IExternalCommands
    {
        IBinUploaderCommands BinUploaderCommands { get; }
        IConsoleCommands ConsoleCommands { get; }
        IUserCmdCommands UserCmdCommands { get; }
        ICircPumpCommands CircPumpCommands { get; }
        ICollectorCommands CollectorCommands { get; }
        IComfortZoneCommands ComfortZoneCommands { get; }
        IKTypeCommands KTypeCommands { get; }
        IRelayModuleCommands RelayModuleCommands { get; }
        ISystemCommands SystemCommands { get; }
        ISystemFanCommands SystemFanCommands { get; }
        ISwitchCommands SwitchCommands { get; }
        ITempSensorCommands TempSensorCommands { get; }
        ITempTriggerCommands TempTriggerCommands { get; }
        IWaterBoilerCommands WaterBoilerCommands { get; }
        IWoodBoilerCommands WoodBoilerCommands { get; }
    }
}
