namespace NSUWatcher.Interfaces.MCUCommands.To
{
    public interface IToMcuCircPumpCommands
    {
        public ICommandToMCU Clicked(string circPumpName);
    }
}
