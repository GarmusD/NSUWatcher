namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ITSensorConfigSnapshot : IMessageFromMcu
    {
        public byte ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
        public int Interval { get; set; }
    }
}
