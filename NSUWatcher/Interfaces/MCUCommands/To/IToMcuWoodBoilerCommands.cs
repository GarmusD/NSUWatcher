using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSUWatcher.Interfaces.MCUCommands.To
{
    public interface IToMcuWoodBoilerCommands
    {
        ICommandToMCU ActionIkurimas(string name);
        ICommandToMCU LadomatSwitchManualMode(string name);
        ICommandToMCU ExhaustFanSwitchManualMode(string name);
    }
}
