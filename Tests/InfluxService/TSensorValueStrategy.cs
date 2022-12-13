using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSUWatcher.Services.InfluxDB;

namespace Tests.InfluxService
{
    [TestClass]
    public class TSensorValueStrategy
    {
        [TestMethod]
        public void LastValueStrategy()
        {
            TemperatureSensorEntity tsEntity = new TemperatureSensorEntity(10.0, ValueStrategyType.LastValue);
            Assert.AreEqual(10.0, tsEntity.Temperature);

            tsEntity.Temperature = 20.5;
            Assert.AreEqual(20.5, tsEntity.Temperature);
        }

        [TestMethod]
        public void AverageValueStrategy()
        {
            TemperatureSensorEntity tsEntity = new TemperatureSensorEntity(10.0, ValueStrategyType.AverageValue);
            tsEntity.Temperature = 20.0;
            tsEntity.Temperature = 30.0;
            Assert.AreEqual(20.0, tsEntity.Temperature);

            tsEntity.Temperature = 10.0;
            tsEntity.Temperature = 20.0;
            tsEntity.Temperature = 30.0;
            Assert.AreEqual(20.0, tsEntity.Temperature);
        }

        [TestMethod]
        public void MaxValueStrategy()
        {
            TemperatureSensorEntity tsEntity = new TemperatureSensorEntity(10.0, ValueStrategyType.MaxValue);
            Assert.AreEqual(10.0, tsEntity.Temperature);

            tsEntity.Temperature = 15.0;
            Assert.AreEqual(15.0, tsEntity.Temperature);

            tsEntity.Temperature = -30.0;
            Assert.AreEqual(15.0, tsEntity.Temperature);

            tsEntity.Temperature = 45.0;
            Assert.AreEqual(45.0, tsEntity.Temperature);

            tsEntity.Temperature = 21.5;
            Assert.AreEqual(45.0, tsEntity.Temperature);
        }
    }
}
