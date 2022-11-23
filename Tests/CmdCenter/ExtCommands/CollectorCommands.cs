using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSU.Shared;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.NSUSystemPart;
using NSU.Shared.Serializer;
using NSUWatcher.CommandCenter;
using System;
using System.Linq;

namespace Tests.CmdCenter.ExtCommands
{
    [TestClass]
    public class CollectorCommands
    {
        private ExternalCommands _extCommands;
        private NsuSerializer _serializer;

        public CollectorCommands()
        {
            _serializer = new NsuSerializer();
            _extCommands = new ExternalCommands(_serializer);
        }

        [TestMethod]
        public void Setup()
        {
            var actuators = new CollectorUpdateContent.Actuator[4] {
                    new CollectorUpdateContent.Actuator { Type = ActuatorType.NC, Channel = 1 },
                    new CollectorUpdateContent.Actuator { Type = ActuatorType.NO, Channel = 2 },
                    new CollectorUpdateContent.Actuator { Type = ActuatorType.NC, Channel = 3 },
                    new CollectorUpdateContent.Actuator { Type = ActuatorType.NO, Channel = 4 }
             };

            CollectorUpdateContent orig = new CollectorUpdateContent(1, true, "collName", "cpName", 4, actuators );

            var cmd = _extCommands.CollectorCommands.Setup(orig.ConfigPos, orig.Enabled, orig.Name, orig.CircPumpName, orig.ActuatorsCount, orig.Actuators);

            Assert.IsNotNull(cmd);
            Assert.AreEqual(JKeys.Collector.TargetName, cmd.Target);
            Assert.AreEqual(JKeys.Generic.Setup, cmd.Action);
            Assert.IsFalse(string.IsNullOrEmpty(cmd.Content));

            CollectorUpdateContent? deserialized = _serializer.Deserialize<CollectorUpdateContent>(cmd.Content);
            Assert.IsNotNull(deserialized);
            Assert.IsTrue(Equals(orig, deserialized.GetValueOrDefault()));
        }

        private bool Equals(CollectorUpdateContent a, CollectorUpdateContent b)
        {
            return 
                a.ConfigPos == b.ConfigPos &&
                a.Enabled == b.Enabled &&
                a.Name == b.Name &&
                a.CircPumpName == b.CircPumpName &&
                a.ActuatorsCount == b.ActuatorsCount &&
                a.Actuators.SequenceEqual(b.Actuators);

        }
    }
}
