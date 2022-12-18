using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.Interfaces.MCUCommands
{
#nullable enable
    // Empty interface to declare McuData
    public interface ICommandToMcuData
    {
        string Target { get; }
        string Action { get; }
        string? CommandId { get; }
    }
#nullable disable
}
