namespace NSUWatcher.Interfaces.MCUCommands
{
    public interface IFromMcuMessages
    {
        IMessageFromMcu Parse(string command);
    }
}