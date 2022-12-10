namespace NSUWatcher.Interfaces.MCUCommands.From
{
    /// <summary>
    /// Contains status of all relay modules
    /// </summary>
    public interface IRelayInfo : IMessageFromMcu
    {   
        IRelayModuleStatus[] Values { get; set; }
    }

    /// <summary>
    /// Status of single relay module
    /// </summary>
    public interface IRelayModuleStatus
    {
        byte StatusFlags { get; set; }
        byte LockFlags { get; set; }
    }
}
