using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSU.Shared;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSUWatcher.CommandCenter;

namespace Tests.CmdCenter.ExtCommands
{
    [TestClass]
    public class KTypeCommands
    {
        private readonly NsuSerializer _serializer;
        private readonly ExternalCommands _extCommands;

        public KTypeCommands()
        {
            _serializer = new NsuSerializer();
            _extCommands = new ExternalCommands(_serializer);
        }

        [TestMethod]
        public void Setup()
        {
            KTypeSetupContent orig = new KTypeSetupContent(1, true, "kTypeName", 15);

            var cmd = _extCommands.KTypeCommands.Setup(orig.ConfigPos, orig.Enabled, orig.Name, orig.Interval);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(JKeys.KType.TargetName, cmd.Target);
            Assert.AreEqual(JKeys.Action.Setup, cmd.Action);
            Assert.IsFalse(string.IsNullOrEmpty(cmd.Content));

            KTypeSetupContent? deserialized = _serializer.Deserialize<KTypeSetupContent>(cmd.Content);
            Assert.IsNotNull(deserialized);
            Assert.IsTrue(orig.Equals(deserialized));
        }
    }
}
