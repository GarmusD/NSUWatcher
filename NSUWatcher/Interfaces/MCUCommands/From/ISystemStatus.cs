namespace NSUWatcher.Interfaces.MCUCommands.From
{
#nullable enable
    public interface ISystemStatus : IMessageFromMcu
    {
        string CurrentState { get; set; }
        int FreeMem { get; set; }
        int? UpTime { get; set; }
        bool RebootRequired { get; set; }
    }
#nullable enable
}
