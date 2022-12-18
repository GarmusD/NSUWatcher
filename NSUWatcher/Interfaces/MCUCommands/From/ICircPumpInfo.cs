namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ICircPumpInfo : IMessageFromMcu
    {
        string Name { get; set; }
        string Status { get; set; }
        byte CurrentSpeed { get; set; }
        byte ValvesOpened { get; set; }
    }
}
