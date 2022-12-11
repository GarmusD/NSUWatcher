namespace NSUWatcher.Interfaces.MCUCommands.From
{
#nullable enable
    public interface ITempTriggerSnapshot : IMessageFromMcu
    {
        byte ConfigPos { get; set; }
        bool Enabled { get; set; }
        string Name { get; set; }
        ITempTriggerPiece[] TempTriggerPieces { get; set; }   
        string? Status { get; set; }
    }
#nullable disable

    public interface ITempTriggerPiece : IMessageFromMcu
    {
        bool Enabled { get; set; }
        string TSensorName { get; set; }
        int Condition { get; set; }
        double Temperature { get; set; }
        double Histeresis { get; set; }
    }
}
