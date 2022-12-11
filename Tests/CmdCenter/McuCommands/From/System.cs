using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace Tests.CmdCenter.McuCommands.From
{
    [TestClass]
    public class System : FromMcuBase
    {
        [TestMethod]
        public void SystemStatusRunning()
        {
            string testJson = "{\"target\":\"system\",\"action\":\"systemstatus\",\"value\":\"running\",\"freemem\":44856,\"uptime\":0,\"rebootreq\":false}";
            Check<ISystemStatus>(testJson);
        }

        [TestMethod]
        public void SnapshotDone()
        {
            string testJson = "{\"target\":\"system\",\"action\":\"snapshot\",\"result\":\"done\"}";
            Check<ISystemSnapshotDone>(testJson);

        }
    }
}
