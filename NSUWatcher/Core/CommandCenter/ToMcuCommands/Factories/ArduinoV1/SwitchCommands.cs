using NSUWatcher.Core.CommandCenter.ToMcuCommands.Factories.ArduinoV1.Data.Switch;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.Interfaces.MCUCommands.To;
using System;

namespace NSUWatcher.Core.CommandCenter.ToMcuCommands.Factories.ArduinoV1
{
    public class SwitchCommands : IToMcuSwitchCommands
    {
        private readonly Action<string> _defaultSendAction;

        public SwitchCommands(Action<string> defaultSendAction)
        {
            _defaultSendAction = defaultSendAction;
        }

        public ICommandToMCU Clicked(string switchName)
        {
            return new CommandToMCU<ClickedData>(_defaultSendAction, new ClickedData(switchName));
        }
    }
}
