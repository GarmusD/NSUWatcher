namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ISystemSetTimeResult : IMessageFromMcu
    {
        public string Result { get; set; }
    }
}
