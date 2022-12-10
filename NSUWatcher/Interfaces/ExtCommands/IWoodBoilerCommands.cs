namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface IWoodBoilerCommands
    {
        public IExternalCommand Setup(byte configPos, string name, string tempSensorName, string ktypeName, byte ladomatChannel, byte exhaustFanChannel,
            double workingTemperature, double workingHisteresis, double ladomatWorkingTemp, string ladomatTempTriggerName, string waterBoilerName);

        public IExternalCommand StartUp(string woodBoilerName = "");
        public IExternalCommand SwitchLadomatManual(string woodBoilerName = "");
        public IExternalCommand SwitchExhaustFanManual(string woodBoilerName = "");
    }
}
