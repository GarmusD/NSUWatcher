namespace NSUWatcher.Interfaces.MCUCommands
{
    public interface IMcuCommands
    {
        public IToMcuCommands ToMcu { get; }
        public IFromMcuMessages FromMcu { get; }
    }
}
