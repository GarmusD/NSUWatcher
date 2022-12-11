namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ISwitchSnapshot : IMessageFromMcu
    {
        public byte ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public string DependOnName { get; set; }
        public string DependancyStatus { get; set; }
        public string ForceState { get; set; }
        public bool? IsForced { get; set; }
        public string? CurrentState { get; set; }
    }
}
