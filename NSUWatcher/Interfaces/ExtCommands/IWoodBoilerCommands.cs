namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface IWoodBoilerCommands
    {
        IExternalCommand Setup(byte configPos, string name, string tempSensorName, string ktypeName, byte ladomatChannel, byte exhaustFanChannel,
            double workingTemperature, double workingHisteresis, double ladomatWorkingTemp, string ladomatTempTriggerName, string waterBoilerName);

        IExternalCommand StartUp(string woodBoilerName = "");
        IExternalCommand SwitchLadomatManual(string woodBoilerName = "");
        IExternalCommand SwitchExhaustFanManual(string woodBoilerName = "");
    }
}
