namespace NSUWatcher.Services.InfluxDB
{
    public interface IValueStrategy<T>
    {
        T Value { get; set; }
    }
}
