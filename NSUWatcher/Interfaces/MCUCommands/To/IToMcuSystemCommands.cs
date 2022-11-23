namespace NSUWatcher.Interfaces.MCUCommands.To
{
    public interface IToMcuSystemCommands
    {
        ICommandToMCU EmptyCommand();
        ICommandToMCU GetMcuStatus();
        ICommandToMCU RequestSnapshot();
        ICommandToMCU PauseBoot();
        ICommandToMCU SetTime(int year, int month, int day, int hour, int minute, int second);
    }
}
