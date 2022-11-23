using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.ExtCommands;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSU.Shared;
using static NSU.Shared.DTO.ExtCommandContent.CollectorUpdateContent;

namespace NSUWatcher.CommandCenter.ExtCommands
{
    public class CollectorCommands : ICollectorCommands
    {
        private readonly INsuSerializer _serializer;
        public CollectorCommands(INsuSerializer serializer)
        {
            _serializer = serializer;
        }

        public IExternalCommand Setup(byte configPos, bool enabled, string name, string circPumpName, byte actuatorsCount, params Actuator[] actuators)
        {
            return new ExternalCommand()
            {
                Target = JKeys.Collector.TargetName,
                Action = JKeys.Generic.Setup,
                Content = _serializer.Serialize( new CollectorUpdateContent(configPos, enabled, name, circPumpName, actuatorsCount, actuators) )
            };
        }
    }
}
