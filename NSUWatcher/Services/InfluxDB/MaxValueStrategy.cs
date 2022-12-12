namespace NSUWatcher.Services.InfluxDB
{
    public class MaxValueStrategy<T> : IValueStrategy<T>
    {
        private T _value = default;
        private readonly object _lock = new object();

        public T Value { get => _value; set => SetValue(value); }

        public MaxValueStrategy(T value = default)
        {
            try
            {
                _value = (T)typeof(T).GetField("MinValue").GetValue(null);
                dynamic prmValue = value;
                if(prmValue != default(T))
                {
                    Value = value;
                }
            }
            catch
            {
                _value = default;
            }
        }

        public void SetValue(T value)
        {
            lock (_lock)
            {
                dynamic newValue = value;
                if(newValue > _value)
                    _value = newValue;
            }
        }
    }
}
