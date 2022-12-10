using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace Tests.CmdCenter.McuCommands.From
{
    [TestClass]
    public class Alarm : FromMcuBase
    {
        [TestMethod]
        public void Snapshot()
        {
            string testJson = "{\"target\":\"alarm\",\"action\":\"snapshot\",\"content\":\"configplus\",\"cfgpos\":0,\"enabled\":1,\"temp\":95,\"hist\":5,\"chdata\":[{\"ch\":18,\"open\":0},{\"ch\":17,\"open\":1},{\"ch\":20,\"open\":1},{\"ch\":14,\"open\":1},{\"ch\":15,\"open\":1},{\"ch\":255,\"open\":0},{\"ch\":255,\"open\":0},{\"ch\":255,\"open\":0}]}";
            Check<IAlarmSnapshot>(testJson);
        }
    }
}
