namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IWoodBoilerExhaustFanInfo : IMessageFromMcu
    {
        string Name { get; set; }
        string ExhaustFanStatus { get; set; }
    }
}
