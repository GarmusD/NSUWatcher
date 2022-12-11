namespace NSUWatcher.Interfaces.MCUCommands.From
{
	public interface IWoodBoilerInfo : IMessageFromMcu
    {
		string Name { get; set; }
		double CurrentTemperature { get; set; }
		string WBStatus { get; set; }
		string TempStatus { get; set; }
    }
}
