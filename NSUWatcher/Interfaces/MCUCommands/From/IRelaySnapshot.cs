namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IRelaySnapshot : IMessageFromMcu
    {
        public byte ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public bool ActiveLow { get; set; }
        public bool Reversed { get; set; }
        public byte? StatusFlags { get; set; }
        public byte? LockFlags { get; set; }
    }
}
