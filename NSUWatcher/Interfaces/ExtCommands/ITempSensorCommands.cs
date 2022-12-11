namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface ITempSensorCommands
    {
        IExternalCommand Setup(byte configPos, bool enabled, byte[] sensorAddress, string name);
    }
}
