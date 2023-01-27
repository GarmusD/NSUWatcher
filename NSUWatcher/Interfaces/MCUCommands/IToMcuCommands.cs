using NSUWatcher.Interfaces.MCUCommands.To;

namespace NSUWatcher.Interfaces.MCUCommands
{
    public interface IToMcuCommands
    {
        IToMcuCircPumpCommands CircPumpCommands { get; }
        IToMcuSwitchCommands SwitchCommands { get; }
        IToMcuSystemCommands SystemCommands { get; }
        IToMcuWoodBoilerCommands WoodBoilerCommands { get; }
    }
}
