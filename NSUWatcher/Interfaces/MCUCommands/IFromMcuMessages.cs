namespace NSUWatcher.Interfaces.MCUCommands
{
    public interface IFromMcuMessages
    {
        public IMessageFromMcu? Parse(string command);
    }
}