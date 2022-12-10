using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace Tests.CmdCenter.McuCommands.From
{
    [TestClass]
    public class Collector : FromMcuBase
    {
        [TestMethod]
        public void Snapshot()
        {
            string testJson = "{\"target\":\"collector\",\"action\":\"snapshot\",\"content\":\"configplus\",\"cfgpos\":0,\"enabled\":1,\"name\":\"1aukstas\",\"cpname\":\"grindu\",\"vcnt\":8,\"valves\":[{\"type\":0,\"ch\":1,\"status\":true},{\"type\":0,\"ch\":2,\"status\":true},{\"type\":0,\"ch\":3,\"status\":true},{\"type\":0,\"ch\":4,\"status\":true},{\"type\":0,\"ch\":5,\"status\":true},{\"type\":0,\"ch\":6,\"status\":true},{\"type\":0,\"ch\":7,\"status\":true},{\"type\":0,\"ch\":8,\"status\":true}]}";
            Check<ICollectorSnapshot>(testJson);
            
            testJson = "{\"target\":\"collector\",\"action\":\"snapshot\",\"content\":\"config\",\"cfgpos\":3,\"enabled\":0,\"name\":\"\",\"cpname\":\"\",\"vcnt\":0,\"valves\":[]}";
            Check<ICollectorSnapshot>(testJson);
        }
    }
}
