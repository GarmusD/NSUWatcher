namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface ITempSensorCommands
    {
        public IExternalCommand Setup(byte configPos, bool enabled, byte[] sensorAddress, string name);
    }
}
