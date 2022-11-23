using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSU.Shared;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSUWatcher.CommandCenter;

namespace Tests.CmdCenter.ExtCommands
{
    [TestClass]
    public class CircPumpCommands
    {
        private const string CircPumpName = "testName";
        private const string TempTriggerName = "testTriggerName";

        private ExternalCommands _extCommands;
        private readonly NsuSerializer _serializer;

        public CircPumpCommands()
        {
            _serializer = new NsuSerializer();
            _extCommands = new ExternalCommands(_serializer);
        }

        [TestMethod]
        public void Clicked()
        {
            var cmd = _extCommands.CircPumpCommands.Clicked(CircPumpName);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(JKeys.CircPump.TargetName, cmd.Target);
            Assert.AreEqual(JKeys.Action.Click, cmd.Action);
            Assert.AreEqual(CircPumpName, cmd.Content);
        }

        [TestMethod]
        public void Setup()
        {
            CircPumpUpdateContent orig = new CircPumpUpdateContent(1, true, CircPumpName, TempTriggerName, 3, 11, 22, 33);

            var cmd = _extCommands.CircPumpCommands.Setup(orig.ConfigPos, orig.Enabled, orig.Name, orig.TempTriggerName, orig.MaxSpeed, orig.Speed1Channel, orig.Speed2Channel, orig.Speed3Channel);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(JKeys.CircPump.TargetName, cmd.Target);
            Assert.AreEqual(JKeys.Generic.Setup, cmd.Action);
            CircPumpUpdateContent? cpUpdSer = _serializer.Deserialize<CircPumpUpdateContent>(cmd.Content);
            Assert.IsNotNull(cpUpdSer);
            Assert.IsTrue(orig.Equals(cpUpdSer.GetValueOrDefault()));
        }
    }
}
