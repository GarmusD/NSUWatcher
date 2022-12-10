using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace Tests.CmdCenter.McuCommands.From
{
    [TestClass]
    public class TSensor : FromMcuBase
    {
        [TestMethod]
        public void Snapshot()
        {
            string testJson = "{\"target\":\"tsensor\",\"action\":\"snapshot\",\"content\":\"system\",\"addr\":\"28:80:1F:94:04:00:00:96\",\"temp\":7.4,\"errors\":0}";
            Check<ITSensorSystemSnapshot>(testJson);

            testJson = "{\"target\":\"tsensor\",\"action\":\"snapshot\",\"content\":\"config\",\"cfgpos\":0,\"enabled\":1,\"addr\":\"28:80:1F:94:04:00:00:96\",\"name\":\"sensorName\",\"interval\":240}";
            Check<ITSensorConfigSnapshot>(testJson);
        }

        [TestMethod]
        public void Info()
        {
            var testJson = "{\"target\":\"tsensor\",\"action\":\"info\",\"addr\":\"28:F1:F0:93:04:00:00:0A\",\"value\":47.8,\"errors\":4}";
            Check<ITSensorInfo>(testJson);
        }
    }
}
