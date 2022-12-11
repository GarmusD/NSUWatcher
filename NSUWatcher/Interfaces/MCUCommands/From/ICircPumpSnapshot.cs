namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ICircPumpSnapshot : IMessageFromMcu
    {
        byte ConfigPos { get; set; }
        bool Enabled { get; set; }
        string Name { get; set; }
        int MaxSpeed { get; set; }
        int Speed1Ch { get; set; }
        int Speed2Ch { get; set; }
        int Speed3Ch { get; set; }
        string TempTriggerName { get; set; }
        string Status { get; set; }
        int? CurrentSpeed { get; set; }
        int? OpenedValvesCount { get; set; }
    }
}
