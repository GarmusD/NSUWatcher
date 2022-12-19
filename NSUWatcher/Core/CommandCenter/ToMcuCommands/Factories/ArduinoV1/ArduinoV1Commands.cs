using System;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.Interfaces.MCUCommands.To;

namespace NSUWatcher.CommandCenter.ToMcuCommands.Factories.ArduinoV1
{
    public class ArduinoV1Commands : IToMcuCommands
    {
        public IToMcuCircPumpCommands CircPumpCommands => _circPumpCommands;
        public IToMcuSystemCommands SystemCommands => _systemCommands;

        private readonly CircPumpCommands _circPumpCommands;
        private readonly SystemCommands _systemCommands;

        public ArduinoV1Commands(Action<string> sendAction)
        {
            _circPumpCommands = new CircPumpCommands(sendAction);
            _systemCommands = new SystemCommands(sendAction);
        }
    }
}
