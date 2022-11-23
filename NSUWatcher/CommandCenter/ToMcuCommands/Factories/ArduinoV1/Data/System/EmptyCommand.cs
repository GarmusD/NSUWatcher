using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands;

namespace NSUWatcher.CommandCenter.ToMcuCommands.Factories.ArduinoV1.Data.System
{
    public struct EmptyCommand : ICommandToMcuData
    {
        public string Target => JKeys.Syscmd.TargetName;

        public string Action { get; }
        public EmptyCommand(string action = "no_action")
        {
            Action = action;
        }
    }
}
