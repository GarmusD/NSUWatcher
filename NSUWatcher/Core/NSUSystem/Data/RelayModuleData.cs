using NSU.Shared.DataContracts;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.NSUSystem.Data
{
    public class RelayModuleData : IRelayModuleDataContract
    {
        public byte ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public bool ActiveLow { get; set; }
        public bool ReversedOrder { get; set; }
        public byte StatusFlags { get; set; }
        public byte LockFlags { get; set; }

        public RelayModuleData(IRelaySnapshot snapshot)
        {
            ConfigPos = snapshot.ConfigPos;
            Enabled = snapshot.Enabled;
            ActiveLow = snapshot.ActiveLow;
            ReversedOrder = snapshot.Reversed;
            StatusFlags = snapshot.StatusFlags.GetValueOrDefault();
            LockFlags = snapshot.LockFlags.GetValueOrDefault();
        }
    }
}