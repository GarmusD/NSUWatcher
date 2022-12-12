using System;

namespace NSUWatcher.Services.InfluxDB
{
    public class AverageValueStrategy<T> : IValueStrategy<T>
    {
        private T _value = default;
        private int _count = 0;
        private readonly object _lock = new object();

        public T Value { get => GetValue(); set => SetValue(value); }

        public AverageValueStrategy(T value = default)
        {
            dynamic prmValue = value;
            if (prmValue != default(T))
            {
                Value = value;
            }
        }

        public void SetValue(T value)
        {
            lock (_lock)
            {
                dynamic val = value;
                _value += val;
                _count++;
            }
        }

        private T GetValue()
        {
            if (_count == 0) return _value;
            dynamic avgValue;
            lock (_lock)
            {
                dynamic sumValue = _value;
                avgValue = sumValue / _count;
                _value = avgValue;
                _count = 1;
            }
            return avgValue;
        }
    }
}
