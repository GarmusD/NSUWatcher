using NSU.Shared.DTO.ExtCommandContent;

namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface ICircPumpCommands
    {
        public IExternalCommand Setup(byte configPos, bool enabled, string name, string tempTriggerName, byte maxSpeed, byte speed1Ch, byte speed2Ch, byte speed3Ch);
        public IExternalCommand Clicked(string name);
    }
}
