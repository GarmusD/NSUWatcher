namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IKTypeSnapshot : IMessageFromMcu
    {
        public byte ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public int Interval { get; set; }
        public int? Temperature { get; set; }
    }
}
