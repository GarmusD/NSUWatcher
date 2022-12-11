namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ITempChanged : IMessageFromMcu
    {
        public string Address { get; set; }
        public float Temperature { get; set; }
        public int ReadErrorCount { get; set; }
    }
}
