using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace Tests.CmdCenter.McuCommands.From
{
    [TestClass]
    public class WoodBoiler : FromMcuBase
    {
        [TestMethod]
        public void Snapshot()
        {
            string testJson = "{\"target\":\"woodboiler\",\"action\":\"snapshot\",\"content\":\"configplus\",\"cfgpos\":0,\"enabled\":1,\"name\":\"default\",\"worktemp\":85,\"hist\":2,\"tsname\":\"katilas\",\"ktpname\":\"dumu_temp\",\"smokech\":18,\"lch\":17,\"ltrgname\":\"ladomat\",\"ltemp\":62,\"wbname\":\"\",\"currtemp\":47.7,\"status\":\"UZGESES\",\"lstatus\":\"OFF\",\"exhstatus\":\"OFF\",\"tstatus\":\"Lowering\"}";
            Check<IWoodBoilerSnapshot>(testJson);
        }

        [TestMethod]
        public void Info()
        {
            string testJson = "{\"target\":\"woodboiler\",\"action\":\"info\",\"name\":\"default\",\"content\":\"exhstatus\",\"value\":\"ON\"}";
            Check<IWoodBoilerExhaustFanInfo>(testJson);
            
            testJson = "{\"target\":\"woodboiler\",\"action\":\"info\",\"name\":\"default\",\"content\":\"woodboiler\",\"currtemp\":47.6,\"status\":\"IKURIAMAS\",\"tstatus\":\"Growing\"}";
            Check<IWoodBoilerInfo>(testJson);
            
            testJson = "{\"target\":\"woodboiler\",\"action\":\"info\",\"name\":\"default\",\"content\":\"lstatus\",\"value\":\"ON\"}";
            Check<IWoodBoilerLadomatInfo>(testJson);
        }
    }
}
