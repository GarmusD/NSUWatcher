using NSU.Shared.DataContracts;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUSystemPart;

namespace NSUWatcher.NSUWatcherNet.NetMessenger
{
    public static class WoodBoilerChanges
    {
        public static NetMessage? Message(IWoodBoilerDataContract dataContract, string property)
        {
            return property switch
            {
                nameof(WoodBoiler.CurrentTemp) => new NetMessage(
                    WoodBoilerTempChanged.Create(dataContract.Name, dataContract.TempStatus.ToString(), dataContract.CurrentTemp)
                    ),
                nameof(WoodBoiler.WBStatus) => new NetMessage(
                    WoodBoilerStatusChanged.CreateForWB(dataContract.Name, dataContract.WBStatus.ToString())
                    ),
                nameof(WoodBoiler.LadomStatus) => new NetMessage(
                    WoodBoilerStatusChanged.CreateForLadomat(dataContract.Name, dataContract.LadomStatus.ToString())
                    ),
                nameof(WoodBoiler.ExhaustFanStatus) => new NetMessage(
                    WoodBoilerStatusChanged.CreateForExhaustFan(dataContract.Name, dataContract.ExhaustFanStatus.ToString())
                    ),
                _ => null
            };
        }
    }
}
