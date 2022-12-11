namespace NSUWatcher.Interfaces.MCUCommands.From
{
    public interface ISystemStatus : IMessageFromMcu
    {
        public string CurrentState { get; set; }
        public int FreeMem { get; set; }
        public int? UpTime { get; set; }
        public bool RebootRequired { get; set; }
    }
}
