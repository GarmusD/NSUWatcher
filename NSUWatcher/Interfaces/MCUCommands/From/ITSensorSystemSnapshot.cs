namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ITSensorSystemSnapshot : IMessageFromMcu
    {
        // Address in format "%02X:%02X:%02X:%02X:%02X:%02X:%02X:%02X"
        string Address { get; set; }
        double Temperature { get; set; }
        int ReadErrors { get; set; }
    }
}
