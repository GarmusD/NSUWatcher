using System;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.Interfaces.MCUCommands.To;
using NSUWatcher.CommandCenter.ToMcuCommands.Factories.ArduinoV1.Data.CircPump;

namespace NSUWatcher.CommandCenter.ToMcuCommands.Factories.ArduinoV1
{
    public class CircPumpCommands : IToMcuCircPumpCommands
    {
        private readonly Action<string> _defaultSendAction;

        public CircPumpCommands(Action<string> sendAction)
        {
            _defaultSendAction = sendAction;
        }

        public ICommandToMCU Clicked(string circPumpName)
        {
            return new CommandToMCU<ClickedData>(_defaultSendAction, new ClickedData(circPumpName));
        }
    }
}
