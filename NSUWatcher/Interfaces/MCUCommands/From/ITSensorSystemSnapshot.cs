namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ITSensorSystemSnapshot : IMessageFromMcu
    {
        // Address in format "%02X:%02X:%02X:%02X:%02X:%02X:%02X:%02X"
        public string Address { get; set; }
        public double Temperature { get; set; }
        public int ReadErrors { get; set; }
    }
}
