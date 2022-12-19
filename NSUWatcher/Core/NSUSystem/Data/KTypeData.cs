using NSU.Shared.DataContracts;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.NSUSystem.Data
{
    public class KTypeData : IKTypeDataContract
    {
        public byte ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public int Interval { get; set; }
        public int Temperature { get; set; }

        public KTypeData(IKTypeSnapshot snapshot)
        {
            ConfigPos = snapshot.ConfigPos;
            Enabled = snapshot.Enabled;
            Name = snapshot.Name;
            Interval = snapshot.Interval;
            Temperature = snapshot.Temperature.GetValueOrDefault();
        }
    }
}