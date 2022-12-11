namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface ISystemFanCommands
    {
        IExternalCommand Setup(byte configPos, bool enabled, string name, string tempSensorName, double minTemperature, double maxTemperature);
    }
}
