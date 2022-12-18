namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ICircPumpSnapshot : IMessageFromMcu
    {
        byte ConfigPos { get; set; }
        bool Enabled { get; set; }
        string Name { get; set; }
        byte MaxSpeed { get; set; }
        byte Speed1Ch { get; set; }
        byte Speed2Ch { get; set; }
        byte Speed3Ch { get; set; }
        string TempTriggerName { get; set; }
        string Status { get; set; }
        byte? CurrentSpeed { get; set; }
        byte? OpenedValvesCount { get; set; }
    }
}
