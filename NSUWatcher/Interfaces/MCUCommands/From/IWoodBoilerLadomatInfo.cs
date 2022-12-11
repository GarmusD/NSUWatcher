namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IWoodBoilerLadomatInfo : IMessageFromMcu
    {
        string Name { get; set; }
        string LadomatStatus { get; set; }
    }
}
