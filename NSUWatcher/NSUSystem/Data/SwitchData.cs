using System;
using NSU.Shared.DataContracts;
using NSU.Shared.NSUSystemPart;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.NSUSystem.Data
{
    public class SwitchData : ISwitchDataContract
    {
        public byte ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public string Dependancy { get; set; }
        public Status Status { get; set; }
        public Status OnDependancyStatus { get; set; }
        public Status ForceStatus { get; set; }
        public bool IsForced { get; set; }

        public SwitchData(ISwitchSnapshot snapshot)
        {
            Enabled = snapshot.Enabled;
            Name = snapshot.Name;
            ConfigPos = snapshot.ConfigPos;
            Dependancy = snapshot.DependOnName;
            Status = snapshot.CurrentState == null ? Status.UNKNOWN : (Status)Enum.Parse(typeof(Status), snapshot.CurrentState, true);
            OnDependancyStatus = (Status)Enum.Parse(typeof(Status), snapshot.DependancyStatus, true);
            ForceStatus = (Status)Enum.Parse(typeof(Status), snapshot.ForceState, true);
            IsForced = snapshot.IsForced.GetValueOrDefault();
        }
    }
}