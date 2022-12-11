namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ICircPumpSnapshot : IMessageFromMcu
    {
        public byte ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public int MaxSpeed { get; set; }
        public int Speed1Ch { get; set; }
        public int Speed2Ch { get; set; }
        public int Speed3Ch { get; set; }
        public string TempTriggerName { get; set; }
        public string? Status { get; set; }
        public int? CurrentSpeed { get; set; }
        public int? OpenedValvesCount { get; set; }
    }
}
