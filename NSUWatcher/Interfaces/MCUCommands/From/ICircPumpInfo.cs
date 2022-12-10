namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ICircPumpInfo : IMessageFromMcu
    {
        string Name { get; set; }
        string Status { get; set; }
        int CurrentSpeed { get; set; }
        int ValvesOpened { get; set; }
    }
}
