using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSUWatcher.Interfaces.MCUCommands.From;
using NSUWatcher.NSUSystem.Data;


namespace Tests.CmdCenter.McuCommands.From
{
    [TestClass]
    public class TempTrigger : FromMcuBase
    {
        [TestMethod]
        public void Snapshot()
        {
            string testJson = "{\"target\":\"trigger\",\"action\":\"snapshot\",\"content\":\"configplus\",\"cfgpos\":0,\"enabled\":1,\"name\":\"ladomat\",\"pieces\":[{\"enabled\":1,\"sname\":\"akum_apacia\",\"cndt\":0,\"temp\":55,\"hist\":1},{\"enabled\":0,\"sname\":\"\",\"cndt\":0,\"temp\":0,\"hist\":0},{\"enabled\":0,\"sname\":\"\",\"cndt\":0,\"temp\":0,\"hist\":0},{\"enabled\":0,\"sname\":\"\",\"cndt\":0,\"temp\":0,\"hist\":0}],\"status\":\"ON\"}";
            Check<ITempTriggerSnapshot>(testJson);
            
            testJson = "{\"target\":\"trigger\",\"action\":\"snapshot\",\"content\":\"config\",\"cfgpos\":4,\"enabled\":0,\"name\":\"\",\"pieces\":[{\"enabled\":0,\"sname\":\"\",\"cndt\":0,\"temp\":0,\"hist\":0},{\"enabled\":0,\"sname\":\"\",\"cndt\":0,\"temp\":0,\"hist\":0},{\"enabled\":0,\"sname\":\"\",\"cndt\":0,\"temp\":0,\"hist\":0},{\"enabled\":0,\"sname\":\"\",\"cndt\":0,\"temp\":0,\"hist\":0}]}";
            Check<ITempTriggerSnapshot>(testJson);
            
            testJson = "{\"target\":\"trigger\",\"action\":\"snapshot\",\"content\":\"config\",\"cfgpos\":15,\"enabled\":0,\"name\":\"\",\"pieces\":[{\"enabled\":0,\"sname\":\"\",\"cndt\":0,\"temp\":0,\"hist\":0},{\"enabled\":0,\"sname\":\"\",\"cndt\":0,\"temp\":0,\"hist\":0},{\"enabled\":0,\"sname\":\"\",\"cndt\":0,\"temp\":0,\"hist\":0},{\"enabled\":0,\"sname\":\"\",\"cndt\":0,\"temp\":0,\"hist\":0}]}";
            
            var snapshot = _messagesFromMcu.Parse(testJson) as ITempTriggerSnapshot;
            var dataContract = new TempTriggerData(snapshot);
            var trg = new NSU.Shared.NSUSystemPart.TempTrigger(dataContract);
            Assert.IsNotNull(trg);
        }

        public void Info()
        {
            string testJson = "{\"target\":\"trigger\",\"action\":\"info\",\"name\":\"ladomat\",\"status\":\"ON\"}";
            Check<ITempTriggerInfo>(testJson);
        }
    }
}
