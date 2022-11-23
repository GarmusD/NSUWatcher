using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSU.Shared;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSUWatcher.CommandCenter;

namespace Tests.CmdCenter.ExtCommands
{
    [TestClass]
    public class RelayModuleCommands
    {
        private readonly NsuSerializer _serializer;
        private readonly ExternalCommands _extCommands;

        public RelayModuleCommands()
        {
            _serializer = new NsuSerializer();
            _extCommands = new ExternalCommands(_serializer);
        }

        [TestMethod]
        public void Setup()
        {
            RelayModuleSetupContent orig = new RelayModuleSetupContent(1, true, true, true);

            var cmd = _extCommands.RelayModuleCommands.Setup(orig.ConfigPos, orig.Enabled, orig.ActiveLow, orig.Reversed);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(JKeys.RelayModule.TargetName, cmd.Target);
            Assert.AreEqual(JKeys.Action.Setup, cmd.Action);
            Assert.IsFalse(string.IsNullOrEmpty(cmd.Content));

            RelayModuleSetupContent? deserialized = _serializer.Deserialize<RelayModuleSetupContent>(cmd.Content);
            Assert.IsNotNull(deserialized);
            Assert.IsTrue(orig.Equals(deserialized.GetValueOrDefault()));
        }

        [TestMethod]
        public void OpenChannel()
        {
            RelayModuleOpenChContent orig = new RelayModuleOpenChContent(true, 5);

            var cmd = _extCommands.RelayModuleCommands.OpenChannel(orig.Channel);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(JKeys.RelayModule.TargetName, cmd.Target);
            Assert.AreEqual(JKeys.RelayModule.ActionOpenChannel, cmd.Action);
            Assert.IsFalse(string.IsNullOrEmpty(cmd.Content));

            RelayModuleOpenChContent? deserialized = (_serializer.Deserialize<RelayModuleOpenChContent>(cmd.Content));
            Assert.IsNotNull(deserialized);
            Assert.IsTrue(orig.Equals(deserialized.GetValueOrDefault()));
        }

        [TestMethod]
        public void CloseChannel()
        {
            RelayModuleOpenChContent orig = new RelayModuleOpenChContent(false, 5);

            var cmd = _extCommands.RelayModuleCommands.CloseChannel(orig.Channel);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(JKeys.RelayModule.TargetName, cmd.Target);
            Assert.AreEqual(JKeys.RelayModule.ActionCloseChannel, cmd.Action);
            Assert.IsFalse(string.IsNullOrEmpty(cmd.Content));

            RelayModuleOpenChContent? deserialized = (_serializer.Deserialize<RelayModuleOpenChContent>(cmd.Content));
            Assert.IsNotNull(deserialized);
            Assert.IsTrue(orig.Equals(deserialized.GetValueOrDefault()));
        }

        [TestMethod]
        public void LockChannel()
        {
            RelayModuleLockChContent orig = new RelayModuleLockChContent(true, 5, true);

            var cmd = _extCommands.RelayModuleCommands.LockChannel(orig.Channel, orig.OpenChannel);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(JKeys.RelayModule.TargetName, cmd.Target);
            Assert.AreEqual(JKeys.RelayModule.ActionLockChannel, cmd.Action);
            Assert.IsFalse(string.IsNullOrEmpty(cmd.Content));

            RelayModuleLockChContent? deserialized = (_serializer.Deserialize<RelayModuleLockChContent>(cmd.Content));
            Assert.IsNotNull(deserialized);
            Assert.IsTrue(orig.Equals(deserialized.GetValueOrDefault()));
        }

        [TestMethod]
        public void UnlockChannel()
        {
            RelayModuleLockChContent orig = new RelayModuleLockChContent(false, 5, false);

            var cmd = _extCommands.RelayModuleCommands.UnlockChannel(orig.Channel);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(JKeys.RelayModule.TargetName, cmd.Target);
            Assert.AreEqual(JKeys.RelayModule.ActionUnlockChannel, cmd.Action);
            Assert.IsFalse(string.IsNullOrEmpty(cmd.Content));

            RelayModuleLockChContent? deserialized = (_serializer.Deserialize<RelayModuleLockChContent>(cmd.Content));
            Assert.IsNotNull(deserialized);
            Assert.IsTrue(orig.Equals(deserialized.GetValueOrDefault()));
        }
    }
}
