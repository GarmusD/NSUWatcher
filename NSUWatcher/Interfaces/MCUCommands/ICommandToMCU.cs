namespace NSUWatcher.Interfaces.MCUCommands
{
    public interface ICommandToMCU
    {
        string Value { get; }
        void Send();
    }
}
