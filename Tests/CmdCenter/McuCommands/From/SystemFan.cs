using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace Tests.CmdCenter.McuCommands.From
{
    [TestClass]
    public class SystemFan : FromMcuBase
    {
        [TestMethod]
        public void Snapshot()
        {
            string testJson = "{\"target\":\"sysfan\",\"action\":\"snapshot\",\"content\":\"configplus\",\"cfgpos\":0,\"enabled\":1,\"name\":\"systemfan\",\"tsname\":\"arduino\",\"mint\":25,\"maxt\":35,\"value\":0}";
            Check<ISystemFanSnapshot>(testJson);
        }
    }
}
