namespace NSUWatcher.Interfaces.MCUCommands.From
{
#nullable enable
    public interface ISwitchSnapshot : IMessageFromMcu
    {
        byte ConfigPos { get; set; }
        bool Enabled { get; set; }
        string Name { get; set; }
        string DependOnName { get; set; }
        string DependancyStatus { get; set; }
        string ForceState { get; set; }
        bool? IsForced { get; set; }
        string? CurrentState { get; set; }
    }
#nullable disable
}
