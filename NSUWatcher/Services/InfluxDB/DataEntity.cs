namespace NSUWatcher.Services.InfluxDB
{
    public enum DataEntityType
    {
        Unknown,
        DS18B20,
        KType,
    }

    public abstract class DataEntity
    {
        abstract public string SensorName { get; set; }
        abstract public string SensorID { get; set; }
        abstract public DataEntityType SensorType { get; set; }
        abstract public int Timing { get; set; }
    }
}
