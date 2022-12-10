using NSU.Shared.DataContracts;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUSystemPart;

namespace NSUWatcher.NSUWatcherNet.NetMessenger
{
    public static class TSensorChanges
    {
        public static NetMessage? Message(ITempSensorDataContract dataContract, string property)
        {
            return property switch
            {
                nameof(TempSensor.Temperature) => new NetMessage(
                    TSensorTempChanged.Create(TempSensor.AddrToString(dataContract.SensorID), dataContract.Temperature, dataContract.ReadErrorCount)
                    ),
                _ => null
            };
        }
    }
}
