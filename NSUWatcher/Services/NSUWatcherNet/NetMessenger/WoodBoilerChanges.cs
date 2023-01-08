using NSU.Shared.DataContracts;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUSystemPart;

namespace NSUWatcher.Services.NSUWatcherNet.NetMessenger
{
    public static class WoodBoilerChanges
    {
        public static NetMessage Message(IWoodBoilerDataContract dataContract, string property)
        {
            switch (property)
            {
                case nameof(WoodBoiler.CurrentTemp):
                    return new NetMessage(
                        WoodBoilerTempChanged.Create(dataContract.Name, dataContract.TempStatus.ToString(), dataContract.CurrentTemp)
                    );
                case nameof(WoodBoiler.WBStatus):
                    return new NetMessage(
                        WoodBoilerStatusChanged.CreateForWB(dataContract.Name, dataContract.WBStatus.ToString())
                    );
                case nameof(WoodBoiler.LadomStatus):
                    return new NetMessage(
                        WoodBoilerStatusChanged.CreateForLadomat(dataContract.Name, dataContract.LadomStatus.ToString())
                    );
                case nameof(WoodBoiler.ExhaustFanStatus):
                    return new NetMessage(
                        WoodBoilerStatusChanged.CreateForExhaustFan(dataContract.Name, dataContract.ExhaustFanStatus.ToString())
                    );
                default: return null;
            };
        }
    }
}
