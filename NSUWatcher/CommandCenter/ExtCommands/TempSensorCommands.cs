using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.ExtCommands;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSU.Shared;

namespace NSUWatcher.CommandCenter.ExtCommands
{
    public class TempSensorCommands : ITempSensorCommands
    {
        private readonly INsuSerializer _serializer;

        public TempSensorCommands(INsuSerializer serializer)
        {
            _serializer = serializer;
        }

        public IExternalCommand Setup(byte configPos, bool enabled, byte[] sensorAddress, string name)
        {
            return new ExternalCommand()
            {
                Target = JKeys.TempSensor.TargetName,
                Action = JKeys.Generic.Setup,
                Content = _serializer.Serialize( new TempSensorSetupContent(configPos, enabled, sensorAddress, name) )
            };
        }
    }
}
