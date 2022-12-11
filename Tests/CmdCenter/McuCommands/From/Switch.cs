using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace Tests.CmdCenter.McuCommands.From
{
    [TestClass]
    public class Switch : FromMcuBase
    {
        [TestMethod]
        public void Snapshot()
        {
            string testJson = "{\"target\":\"switch\",\"action\":\"snapshot\",\"content\":\"configplus\",\"cfgpos\":0,\"enabled\":1,\"name\":\"winter_mode\",\"dname\":\"\",\"depstate\":\"OFF\",\"forcestate\":\"OFF\",\"isforced\":false,\"currstate\":\"ON\"}";
            Check<ISwitchSnapshot>(testJson);
            
            testJson = "{\"target\":\"switch\",\"action\":\"snapshot\",\"content\":\"config\",\"cfgpos\":4,\"enabled\":0,\"name\":\"\",\"dname\":\"\",\"depstate\":\"OFF\",\"forcestate\":\"OFF\"}";
            Check<ISwitchSnapshot>(testJson);
        }
    }
}
