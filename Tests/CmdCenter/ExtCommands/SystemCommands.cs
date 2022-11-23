using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSU.Shared;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSUWatcher.CommandCenter;
using System;

namespace Tests.CmdCenter.ExtCommands
{
    [TestClass]
    public class SystemCommands
    {
        private readonly NsuSerializer _serializer;
        private readonly ExternalCommands _extCommands;

        public SystemCommands()
        {
            _serializer = new NsuSerializer();
            _extCommands = new ExternalCommands(_serializer);
        }

        [TestMethod]
        public void ResetMcuCommand()
        {
            foreach (ResetType item in Enum.GetValues(typeof(ResetType)))
            {
                TestForResetType(item);
            }

        }

        private void TestForResetType(ResetType resetType)
        {
            SysResetContent orig = new SysResetContent(resetType);

            var cmd = _extCommands.SystemCommands.ResetMcu(orig.ResetType);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(JKeys.Syscmd.TargetName, cmd.Target);
            Assert.AreEqual(JKeys.Syscmd.RebootSystem, cmd.Action);
            Assert.IsFalse(string.IsNullOrEmpty(cmd.Content));

            SysResetContent? deserialized = _serializer.Deserialize<SysResetContent>(cmd.Content);
            Assert.IsNotNull(deserialized);
            Assert.IsTrue(orig.Equals(deserialized.GetValueOrDefault()));
        }
    }
}
