using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace Tests.CmdCenter.McuCommands.From
{
    [TestClass]
    public class Relay : FromMcuBase
    {
        [TestMethod]
        public void Snapshot()
        {
            string testJson = "{\"target\":\"relay\",\"action\":\"snapshot\",\"content\":\"configplus\",\"cfgpos\":0,\"enabled\":1,\"activelow\":1,\"inverted\":0,\"flags\":0}";
            Check<IRelaySnapshot>(testJson);
            
            testJson = "{\"target\":\"relay\",\"action\":\"snapshot\",\"content\":\"config\",\"cfgpos\":3,\"enabled\":0,\"activelow\":0,\"inverted\":0}";
            Check<IRelaySnapshot>(testJson);
        }

        [TestMethod]
        public void Opened()
        {
            string testJson = "{\"target\":\"relay\",\"action\":\"open\",\"value\":17,\"locked\":false}";
            Check<IRelayChannelOpened>(testJson);
        }
        
        [TestMethod]
        public void Closed()
        {
            string testJson = "{\"target\":\"relay\",\"action\":\"close\",\"value\":18,\"locked\":false}";
            Check<IRelayChannelClosed>(testJson);
        }
    }
}
