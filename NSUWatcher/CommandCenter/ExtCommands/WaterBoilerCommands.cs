using NSU.Shared;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.ExtCommands;

namespace NSUWatcher.CommandCenter.ExtCommands
{
    public class WaterBoilerCommands : IWaterBoilerCommands
    {
        private readonly INsuSerializer _serializer;

        public WaterBoilerCommands(INsuSerializer serializer)
        {
            _serializer = serializer;
        }

        public IExternalCommand Setup(byte configPos, bool enabled, string name, string tempSensorName, string tempTriggerName, string circPumpName, bool elHeatingEnabled, byte elPowerChannel, params WaterBoilerSetupContent.ElHeatingTime[] elHeatingTime)
        {
            return new ExternalCommand()
            {
                Target = JKeys.WaterBoiler.TargetName,
                Action = JKeys.Generic.Setup,
                Content = _serializer.Serialize(
                    new WaterBoilerSetupContent(configPos, enabled, name, tempSensorName, tempTriggerName, circPumpName, elHeatingEnabled, elPowerChannel, elHeatingTime)
                )
            };
        }
    }
}
