using static NSU.Shared.DTO.ExtCommandContent.CollectorUpdateContent;

namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface ICollectorCommands
    {
        IExternalCommand Setup(byte configPos, bool enabled, string name, string circPumpName, byte actuatorsCount, params Actuator[] actuators);
    }
}
