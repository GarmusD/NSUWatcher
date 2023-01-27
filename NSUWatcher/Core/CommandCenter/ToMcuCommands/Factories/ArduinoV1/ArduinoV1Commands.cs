using System;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.Interfaces.MCUCommands.To;

namespace NSUWatcher.Core.CommandCenter.ToMcuCommands.Factories.ArduinoV1
{
    public class ArduinoV1Commands : IToMcuCommands
    {
        public IToMcuCircPumpCommands CircPumpCommands { get; }
        public IToMcuSystemCommands SystemCommands { get; }
        public IToMcuWoodBoilerCommands WoodBoilerCommands { get; }

        public IToMcuSwitchCommands SwitchCommands { get; }

        public ArduinoV1Commands(Action<string> sendAction)
        {
            CircPumpCommands = new CircPumpCommands(sendAction);
            SwitchCommands= new SwitchCommands(sendAction);
            SystemCommands = new SystemCommands(sendAction);
            WoodBoilerCommands = new WoodBoilerCommands(sendAction);
        }
    }
}
