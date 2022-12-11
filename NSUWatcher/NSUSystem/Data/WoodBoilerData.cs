using NSU.Shared.DataContracts;
using NSU.Shared.NSUSystemPart;
using NSUWatcher.Interfaces.MCUCommands.From;
using System;

namespace NSUWatcher.NSUSystem.Data
{
    public class WoodBoilerData : IWoodBoilerDataContract
    {
        public byte ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TSensorName { get; set; } = string.Empty;
        public string KTypeName { get; set; } = string.Empty;
        public int LadomChannel { get; set; }
        public Status LadomStatus { get; set; }
        public bool LadomatIsManual { get; set; }
        public int ExhaustFanChannel { get; set; }
        public Status ExhaustFanStatus { get; set; }
        public bool ExhaustFanIsManual { get; set; }
        public double WorkingTemp { get; set; }
        public double Histeresis { get; set; }
        public WoodBoilerStatus WBStatus { get; set; }
        public double CurrentTemp { get; set; }
        public WoodBoilerTempStatus TempStatus { get; set; }
        public double LadomatTemp { get; set; }
        public string LadomatTriggerName { get; set; } = string.Empty;
        public string WaterBoilerName { get; set; } = string.Empty;

        public WoodBoilerData(IWoodBoilerSnapshot snapshot)
        {
            ConfigPos = snapshot.ConfigPos;
            Enabled = snapshot.Enabled;
            Name = snapshot.Name;
            TSensorName = snapshot.TempSensorName;
            KTypeName = snapshot.KTypeName;
            LadomChannel = snapshot.LadomChannel;
            LadomStatus = snapshot.LadomatStatus == null ? Status.UNKNOWN : Enum.Parse<Status>(snapshot.LadomatStatus, true);
            ExhaustFanChannel = snapshot.ExhaustFanChannel;
            ExhaustFanStatus = snapshot.ExhaustFanStatus == null ? Status.UNKNOWN : Enum.Parse<Status>(snapshot.ExhaustFanStatus, true);
            WorkingTemp = snapshot.WorkingTemperature;
            Histeresis = snapshot.Histeresis;
            WBStatus = snapshot.Status == null ? WoodBoilerStatus.UNKNOWN : Enum.Parse<WoodBoilerStatus>(snapshot.Status);
            CurrentTemp = snapshot.CurrentTemperature.GetValueOrDefault();
            TempStatus = snapshot.TemperatureStatus == null ? WoodBoilerTempStatus.Stable : Enum.Parse<WoodBoilerTempStatus>(snapshot.TemperatureStatus, true);
            LadomatTemp = snapshot.LadomatWorkingTemp;
            LadomatTriggerName = snapshot.LadomatTriggerName;
            WaterBoilerName = snapshot.WaterBoilerName;
        }
    }
}
