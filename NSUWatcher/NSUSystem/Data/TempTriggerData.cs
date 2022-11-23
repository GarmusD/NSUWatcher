using NSU.Shared.DataContracts;
using NSU.Shared.NSUSystemPart;
using NSUWatcher.Interfaces.MCUCommands.From;
using System;
using System.Threading;

namespace NSUWatcher.NSUSystem.Data
{
    public class TempTriggerData : ITempTriggerDataContract
    {
        public int ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; } = string.Empty;
        public Status Status { get; set; }

        public ITempTriggerPieceDataContract[] TempTriggerPieces { get; }

        public TempTriggerData()
        {
            TempTriggerPieces = new TempTriggerPieceData[ITempTriggerDataContract.MAX_TEMPTRIGGERPIECES];
        }

        public TempTriggerData(ITempTriggerSnapshot snapshot)
        {
            ConfigPos = snapshot.ConfigPos;
            Enabled = snapshot.Enabled;
            Name = snapshot.Name;
            Status = snapshot.Status == null ? Status.UNKNOWN : Enum.Parse<Status>(snapshot.Status, true);
            TempTriggerPieces = new TempTriggerPieceData[ITempTriggerDataContract.MAX_TEMPTRIGGERPIECES];
            for (var i = 0; i < ITempTriggerDataContract.MAX_TEMPTRIGGERPIECES; i++)
            {
                TempTriggerPieceData piece = new TempTriggerPieceData()
                {
                    Index = i,
                    Enabled = snapshot.TempTriggerPieces[i].Enabled,
                    TSensorName = snapshot.TempTriggerPieces[i].TSensorName,
                    Condition = (TriggerCondition)snapshot.TempTriggerPieces[i].Condition,
                    Temperature = snapshot.TempTriggerPieces[i].Temperature,
                    Histeresis = snapshot.TempTriggerPieces[i].Histeresis
                };
                TempTriggerPieces[i] = piece;
            }
        }
    }

    public class TempTriggerPieceData : ITempTriggerPieceDataContract
    {
        public int Index { get; set; }
        public bool Enabled { get; set; }
        public string TSensorName { get; set; } = string.Empty;
        public TriggerCondition Condition { get; set; }
        public double Temperature { get; set; }
        public double Histeresis { get; set; }
    }
}
