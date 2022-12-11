namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ITempTriggerPiece : IMessageFromMcu
    {
        public bool Enabled { get; set; }
        public string TSensorName { get; set; }
        public int Condition { get; set; }
        public double Temperature { get; set; }
        public double Histeresis { get; set; }
    }
}
