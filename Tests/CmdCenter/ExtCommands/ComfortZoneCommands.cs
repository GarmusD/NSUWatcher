using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSU.Shared;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSUWatcher.CommandCenter;

namespace Tests.CmdCenter.ExtCommands
{
    [TestClass]
    public class ComfortZoneCommands
    {
        private readonly ExternalCommands _extCommands;
        private readonly NsuSerializer _serializer;
        public ComfortZoneCommands()
        {
            _serializer = new NsuSerializer();
            _extCommands = new ExternalCommands(_serializer);
        }

        [TestMethod]
        public void Setup()
        {
            ComfortZoneSetupContent orig = new ComfortZoneSetupContent(1, true, "czName", "czTitle", "roomTSName", "floorTSName", "collName", 21.5, 10.5, 25.0, 12.0, 1.0, 5, true);

            var cmd = _extCommands.ComfortZoneCommands.Setup(orig.ConfigPos, orig.Enabled, orig.Name, orig.Title, orig.RoomTempSensor, orig.FloorTempSensor, orig.Collector,
                orig.RoomTempHi, orig.RoomTempLo, orig.FloorTemoHi, orig.FloorTempLo, orig.Histerezis, orig.Actuator, orig.LowTempMode);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(JKeys.ComfortZone.TargetName, cmd.Target);
            Assert.AreEqual(JKeys.Action.Setup, cmd.Action);
            Assert.IsFalse(string.IsNullOrEmpty(cmd.Content));

            ComfortZoneSetupContent? deserialized = _serializer.Deserialize<ComfortZoneSetupContent>(cmd.Content);
            Assert.IsNotNull(deserialized);
            Assert.IsTrue(orig.Equals(deserialized.GetValueOrDefault()));
        }

        [TestMethod]
        public void Update()
        {
            ComfortZoneUpdateContent orig = new ComfortZoneUpdateContent(1, 25.0, null, 15.0, null, true);

            var cmd = _extCommands.ComfortZoneCommands.Update(orig.ConfigPos, orig.RoomTempHi, orig.RoomTempLo, orig.FloorTemoHi, orig.FloorTempLo, orig.LowTempMode);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(JKeys.ComfortZone.TargetName, cmd.Target);
            Assert.AreEqual(JKeys.Action.Update, cmd.Action);
            Assert.IsFalse(string.IsNullOrEmpty(cmd.Content));

            ComfortZoneUpdateContent? deserialized = _serializer.Deserialize<ComfortZoneUpdateContent>(cmd.Content);
            Assert.IsNotNull(deserialized);
            Assert.IsTrue(orig.Equals(deserialized.GetValueOrDefault()));
        }
    }
}
