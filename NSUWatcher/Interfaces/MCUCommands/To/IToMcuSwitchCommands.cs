namespace NSUWatcher.Interfaces.MCUCommands.To
{
    public interface IToMcuSwitchCommands
    {
        ICommandToMCU Clicked(string switchName);
    }
}
