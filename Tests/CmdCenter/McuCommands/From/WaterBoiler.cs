using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace Tests.CmdCenter.McuCommands.From
{
    [TestClass]
    public class WaterBoiler : FromMcuBase
    {
        [TestMethod]
        public void Snapshot()
        {
            string testJson = "{\"target\":\"waterboiler\",\"action\":\"snapshot\",\"content\":\"config\",\"cfgpos\":0,\"enabled\":1,\"name\":\"default\",\"tsname\":\"boileris_virsus\",\"cpname\":\"boilerio\",\"trgname\":\"boilerio\",\"elhenabled\":0,\"powerch\":255,\"powerdata\":[{\"starth\":0,\"startm\":0,\"stoph\":0,\"stopm\":0},{\"starth\":0,\"startm\":0,\"stoph\":0,\"stopm\":0},{\"starth\":0,\"startm\":0,\"stoph\":0,\"stopm\":0},{\"starth\":0,\"startm\":0,\"stoph\":0,\"stopm\":0},{\"starth\":0,\"startm\":0,\"stoph\":0,\"stopm\":0},{\"starth\":0,\"startm\":0,\"stoph\":0,\"stopm\":0},{\"starth\":0,\"startm\":0,\"stoph\":0,\"stopm\":0}]}";
            Check<IWaterBoilerSnapshot>(testJson);
        }
    }
}
