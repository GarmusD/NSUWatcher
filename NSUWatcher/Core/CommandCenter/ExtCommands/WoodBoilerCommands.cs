using NSU.Shared;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.ExtCommands;

namespace NSUWatcher.CommandCenter.ExtCommands
{
    public class WoodBoilerCommands : IWoodBoilerCommands
    {
        private readonly INsuSerializer _serializer;

        public WoodBoilerCommands(INsuSerializer serializer)
        {
            _serializer = serializer;
        }

        public IExternalCommand Setup(byte configPos, string name, string tempSensorName, string ktypeName, byte ladomatChannel, byte exhaustFanChannel, 
            double workingTemperature, double workingHisteresis, double ladomatWorkingTemp, string ladomatTempTriggerName, string waterBoilerName)
        {
            return new ExternalCommand() 
            { 
                Target = JKeys.WoodBoiler.TargetName,
                Action = JKeys.Generic.Setup,
                Content = _serializer.Serialize(
                    new WoodBoilerSetupContent(configPos, name, tempSensorName, ktypeName, ladomatChannel, exhaustFanChannel, workingTemperature, 
                        workingHisteresis, ladomatWorkingTemp, ladomatTempTriggerName, waterBoilerName)
                )
            };
        }

        public IExternalCommand StartUp(string woodBoilerName = "default")
        {
            return new ExternalCommand() 
            {
                Target = JKeys.WoodBoiler.TargetName,
                Action = JKeys.WoodBoiler.ActionIkurimas,
                Content = _serializer.Serialize( new WoodBoilerStartUpContent(woodBoilerName) )
            };
        }

        public IExternalCommand SwitchExhaustFanManual(string woodBoilerName = "default")
        {
            return new ExternalCommand()
            {
                Target = JKeys.WoodBoiler.TargetName,
                Action = JKeys.WoodBoiler.ActionSwitch,
                Content = _serializer.Serialize( new WoodBoilerSwitchManualMode(woodBoilerName, JKeys.WoodBoiler.TargetExhaustFan) )
            };
        }

        public IExternalCommand SwitchLadomatManual(string woodBoilerName = "default")
        {
            return new ExternalCommand()
            {
                Target = JKeys.WoodBoiler.TargetName,
                Action = JKeys.WoodBoiler.ActionSwitch,
                Content = _serializer.Serialize( new WoodBoilerSwitchManualMode(woodBoilerName, JKeys.WoodBoiler.TargetLadomat) )
            };
        }
    }
}
