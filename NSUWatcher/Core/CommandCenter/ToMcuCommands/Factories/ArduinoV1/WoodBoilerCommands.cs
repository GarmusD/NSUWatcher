using NSU.Shared;
using NSUWatcher.Core.CommandCenter.ToMcuCommands.Factories.ArduinoV1.Data.WoodBoiler;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.Interfaces.MCUCommands.To;
using System;

namespace NSUWatcher.Core.CommandCenter.ToMcuCommands.Factories.ArduinoV1
{
    public class WoodBoilerCommands : IToMcuWoodBoilerCommands
    {
        private readonly Action<string> _defaultSendAction;

        public WoodBoilerCommands(Action<string> sendAction)
        {
            _defaultSendAction = sendAction;
        }

        public ICommandToMCU ActionIkurimas(string name)
        {
            return new CommandToMCU<ActionIkurimasData>(_defaultSendAction, new ActionIkurimasData(name, null));
        }

        public ICommandToMCU ExhaustFanSwitchManualMode(string name)
        {
            return new CommandToMCU<SwitchManualModeData>(_defaultSendAction, new SwitchManualModeData(name, JKeys.WoodBoiler.TargetExhaustFan));
        }

        public ICommandToMCU LadomatSwitchManualMode(string name)
        {
            return new CommandToMCU<SwitchManualModeData>(_defaultSendAction, new SwitchManualModeData(name, JKeys.WoodBoiler.TargetLadomat));
        }
    }
}
