namespace NSUWatcher.Interfaces.MCUCommands.From
{
#nullable enable
    public interface ITempTriggerSnapshot : IMessageFromMcu
    {
        public byte ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public ITempTriggerPiece[] TempTriggerPieces { get; set; }
        public string? Status { get; set; }
    }
#nullable disable
}
