namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ISystemError : IMessageFromMcu
    {
        string ErrorValue { get; set; }
    }
}
