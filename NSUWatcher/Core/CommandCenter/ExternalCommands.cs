using NSU.Shared.Serializer;
using NSUWatcher.CommandCenter.ExtCommands;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.ExtCommands;

namespace NSUWatcher.CommandCenter
{
    public class ExternalCommands : IExternalCommands
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

        public ExternalCommands(INsuSerializer serializer)
        {
            BinUploaderCommands = new BinUploaderCommands(serializer);
            ConsoleCommands = new ConsoleCommands(serializer);
            CircPumpCommands = new CircPumpCommands(serializer);
            CollectorCommands = new CollectorCommands(serializer);
            ComfortZoneCommands = new ComfortZoneCommands(serializer);
            KTypeCommands = new KTypeCommands(serializer);
            RelayModuleCommands = new RelayModuleCommands(serializer);
            SystemFanCommands = new SystemFanCommands(serializer);
            SwitchCommands = new SwitchCommands(serializer);
            SystemCommands = new SystemCommands(serializer);
            TempSensorCommands = new TempSensorCommands(serializer);
            TempTriggerCommands = new TempTriggerCommands(serializer);
            WaterBoilerCommands = new WaterBoilerCommands(serializer);
            WoodBoilerCommands = new WoodBoilerCommands(serializer);
            UserCmdCommands = new UserCmdCommands(serializer);
        }
    }
}
