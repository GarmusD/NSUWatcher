using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.ExtCommands;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSU.Shared;

namespace NSUWatcher.CommandCenter.ExtCommands
{
    public class SystemFanCommands : ISystemFanCommands
    {
        private readonly INsuSerializer _serializer;

        public SystemFanCommands(INsuSerializer serializer)
        {
            _serializer = serializer;
        }

        public IExternalCommand Setup(byte configPos, bool enabled, string name, string tempSensorName, double minTemperature, double maxTemperature)
        {
            return new ExternalCommand()
            {
                Target = JKeys.SystemFan.TargetName,
                Action = JKeys.Generic.Setup,
                Content = _serializer.Serialize( new SystemFanSetupContent(configPos, enabled, name, tempSensorName, minTemperature, maxTemperature) )
            };
        }
    }
}
