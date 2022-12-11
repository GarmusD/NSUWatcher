namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ITSensorInfo : IMessageFromMcu
    {
        string SensorID { get; set; }
		double Temperature { get; set; }
        int ReadErrorCount { get; set; }
    }
}
