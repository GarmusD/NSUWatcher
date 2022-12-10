using static NSU.Shared.DTO.ExtCommandContent.WaterBoilerSetupContent;

namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface IWaterBoilerCommands
    {
        public IExternalCommand Setup(byte configPos, bool enabled, string name, string tempSensorName, string tempTriggerName,
            string circPumpName, bool elHeatingEnabled, byte elPowerChannel, params ElHeatingTime[] elHeatingTime);
    }
}
