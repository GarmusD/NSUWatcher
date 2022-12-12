using System;

namespace NSUWatcher.Services.InfluxDB
{
    public class TemperatureSensorEntity : DataEntity
    {
        private readonly IValueStrategy<double> _tempValueStrategy;
        private readonly IValueStrategy<double> _pressValueStrategy;
        private readonly IValueStrategy<int> _humValueStrategy;

        public override string SensorName { get; set; } = string.Empty;
        public override string SensorID { get; set; } = string.Empty;
        public override DataEntityType SensorType { get; set; } = DataEntityType.Unknown;
        public override int Timing { get; set; } = 15;
        public double Temperature { get => _tempValueStrategy.Value; set => _tempValueStrategy.Value = value; }
        public double Pressure { get => _pressValueStrategy.Value; set => _pressValueStrategy.Value = value; }
        public int Humidity { get => _humValueStrategy.Value; set => _humValueStrategy.Value = value; }
        

        public TemperatureSensorEntity(  double temperature, ValueStrategyType temperatureValueStrategy = ValueStrategyType.Default,
                                    double pressure = default, ValueStrategyType pressureValueStrategy = ValueStrategyType.Default,
                                    int humidity = default, ValueStrategyType humidityValueStrategy = ValueStrategyType.Default)
        {
            _tempValueStrategy = GetStrategy(temperature, temperatureValueStrategy);
            _pressValueStrategy = GetStrategy(pressure, pressureValueStrategy);
            _humValueStrategy = GetStrategy(humidity, humidityValueStrategy);
        }

        private IValueStrategy<T> GetStrategy<T>(T value, ValueStrategyType strategy)
        {
            return strategy switch
            {
                ValueStrategyType.Default => new LastValueStrategy<T>(value),
                ValueStrategyType.LastValue => new LastValueStrategy<T>(value),
                ValueStrategyType.AverageValue => new AverageValueStrategy<T>(value),
                ValueStrategyType.MaxValue => new MaxValueStrategy<T>(value),
                _ => throw new NotImplementedException()
            };
        }
    }

    public enum ValueStrategyType
    {
        Default,
        LastValue,
        AverageValue,
        MaxValue
    }
}
