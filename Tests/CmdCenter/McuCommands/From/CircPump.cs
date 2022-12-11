using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace Tests.CmdCenter.McuCommands.From
{
    [TestClass]
    public class CircPump : FromMcuBase
    {
        [TestMethod]
        public void Snapshot()
        {
            string testJson = "{\"target\":\"cpump\",\"action\":\"snapshot\",\"content\":\"configplus\",\"cfgpos\":0,\"enabled\":1,\"name\":\"grindu\",\"maxspd\":1,\"spd1\":19,\"spd2\":255,\"spd3\":255,\"trgname\":\"grindu\",\"status\":\"ON\",\"speed\":1,\"vopened\":8}";
            Check<ICircPumpSnapshot>(testJson);
            
            testJson = "{\"target\":\"cpump\",\"action\":\"snapshot\",\"content\":\"config\",\"cfgpos\":3,\"enabled\":0,\"name\":\"\",\"maxspd\":1,\"spd1\":255,\"spd2\":255,\"spd3\":255,\"trgname\":\"\"}";
            Check<ICircPumpSnapshot>(testJson);
        }
    }
}
