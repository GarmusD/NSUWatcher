using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace Tests.CmdCenter.McuCommands.From
{
    [TestClass]
    public class ComfortZone : FromMcuBase
    {
        [TestMethod]
        public void Snapshot()
        {
            string testJson = "{\"target\":\"czone\",\"action\":\"snapshot\",\"content\":\"configplus\",\"cfgpos\":0,\"enabled\":1,\"name\":\"czName\",\"title\":\"CZ Name\",\"clname\":\"colName\",\"act\":1,\"hist\":0.5,\"rsname\":\"roomTsName\",\"rth\":21,\"rtl\":10,\"fsname\":\"\",\"fth\":24,\"ftl\":10,\"lowmode\":0,\"crt\":8.1,\"cft\":-127,\"actopened\":true}";
            Check<IComfortZoneSnapshot>(testJson);
            
            testJson = "{\"target\":\"czone\",\"action\":\"snapshot\",\"content\":\"config\",\"cfgpos\":10,\"enabled\":0,\"name\":\"\",\"title\":\"\",\"clname\":\"\",\"act\":255,\"hist\":0,\"rsname\":\"\",\"rth\":0,\"rtl\":0,\"fsname\":\"\",\"fth\":0,\"ftl\":0,\"lowmode\":0}";
            Check<IComfortZoneSnapshot>(testJson);
        }

        [TestMethod]
        public void Info()
        {
            string testJson = "{\"target\":\"czone\",\"action\":\"info\",\"name\":\"czName\",\"content\":\"crt\",\"value\":7.4}";
            Check<IComfortZoneRoomTempInfo>(testJson);
        }
    }
}
