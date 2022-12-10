using NSUWatcher.Interfaces.MCUCommands.To;

namespace NSUWatcher.Interfaces.MCUCommands
{
    public interface IToMcuCommands
    {
        public IToMcuCircPumpCommands CircPumpCommands { get; }
        public IToMcuSystemCommands SystemCommands { get; }
    }
}
