namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface ISystemFanCommands
    {
        public IExternalCommand Setup(byte configPos, bool enabled, string name, string tempSensorName, double minTemperature, double maxTemperature);
    }
}
