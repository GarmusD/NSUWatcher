namespace NSUWatcher.Interfaces.MCUCommands
{
    public interface ICommandToMCU
    {
        public string Value { get; }
        public void Send();
    }
}
