namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface IKTypeInfo : IMessageFromMcu
    {
        public string Name { get; set; }
        double Value { get; set; }
    }
}
