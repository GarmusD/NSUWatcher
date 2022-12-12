namespace NSUWatcher.Services.InfluxDB
{
    public class LastValueStrategy<T> : IValueStrategy<T>
    {
        private T _value;
        public T Value { get => _value; set => _value = value; }

        public LastValueStrategy(T value = default)
        {
            _value = value;
        }
    }
}
