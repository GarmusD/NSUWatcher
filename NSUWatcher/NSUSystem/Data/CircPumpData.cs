using NSU.Shared.DataContracts;
using NSU.Shared.NSUSystemPart;
using NSUWatcher.Interfaces.MCUCommands.From;
using System;

namespace NSUWatcher.NSUSystem.Data
{
    public class CircPumpData : ICircPumpDataContract
    {
        public byte ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TempTriggerName { get; set; } = string.Empty;
        public int CurrentSpeed { get; set; }
        public int MaxSpeed { get; set; }
        public int Spd1Channel { get; set; }
        public int Spd2Channel { get; set; }
        public int Spd3Channel { get; set; }
        public Status Status { get; set; }
        public int OpenedValvesCount { get; set; }

        public CircPumpData()
        {

        }

        public CircPumpData(ICircPumpSnapshot snapshot)
        {
            ConfigPos = snapshot.ConfigPos;
            Enabled = snapshot.Enabled;
            Name = snapshot.Name;
            TempTriggerName = snapshot.TempTriggerName;
            CurrentSpeed = snapshot.CurrentSpeed.GetValueOrDefault();
            MaxSpeed = snapshot.MaxSpeed;
            Spd1Channel = snapshot.Speed1Ch;
            Spd2Channel = snapshot.Speed2Ch;
            Spd3Channel = snapshot.Speed3Ch;
            Status = snapshot.Status == null ? Status.UNKNOWN : Enum.Parse<Status>(snapshot.Status, true);
            OpenedValvesCount = snapshot.OpenedValvesCount.GetValueOrDefault();
        }
    }
}
