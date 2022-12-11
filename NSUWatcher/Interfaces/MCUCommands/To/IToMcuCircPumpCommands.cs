namespace NSUWatcher.Interfaces.MCUCommands.To
{
    public interface IToMcuCircPumpCommands
    {
        ICommandToMCU Clicked(string circPumpName);
    }
}
