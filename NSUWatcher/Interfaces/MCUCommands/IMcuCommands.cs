namespace NSUWatcher.Interfaces.MCUCommands
{
    public interface IMcuCommands
    {
        IToMcuCommands ToMcu { get; }
        IFromMcuMessages FromMcu { get; }
    }
}
