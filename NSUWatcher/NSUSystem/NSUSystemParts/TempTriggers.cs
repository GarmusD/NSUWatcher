using System;
using System.Collections.Generic;
using NSU.Shared.NSUSystemPart;
using System.Linq;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.Interfaces.MCUCommands.From;
using NSUWatcher.NSUSystem.Data;
using NSUWatcher.Interfaces;
using NSU.Shared;
using NSU.Shared.Serializer;
using Microsoft.Extensions.Logging;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class TempTriggers : NSUSysPartBase
    {
        public override string[] SupportedTargets => new string[] { JKeys.TempTrigger.TargetName, "TRIGGER:" };

        readonly List<TempTrigger> _triggers = new List<TempTrigger>();

        public TempTriggers(NsuSystem sys, ILoggerFactory loggerFactory, INsuSerializer serializer) : base(sys, loggerFactory, serializer, PartType.TempTriggers) { }

        private TempTrigger FindTrigger(string name)
        {
            return _triggers.FirstOrDefault(x => x.Name == name);
        }

        public override bool ProcessCommandFromMcu(IMessageFromMcu command)
        {
            switch (command)
            {
                case ITempTriggerSnapshot snapshot:
                    ProcessSnapshot(snapshot);
                    return true;

                case ITempTriggerInfo triggerInfo:
                    ProcessInfo(triggerInfo);
                    return true;

                default:
                    return false;
            }
        }

        private void ProcessInfo(ITempTriggerInfo triggerInfo)
        {
            var trg = FindTrigger(triggerInfo.Name);
            if (trg != null)
            {
                trg.Status = (Status)Enum.Parse(typeof(Status), triggerInfo.Status, true);
            }
        }

        private void ProcessSnapshot(ITempTriggerSnapshot snapshot)
        {
            var dataContract = new TempTriggerData(snapshot);
            var trg = new TempTrigger(dataContract);
            trg.AttachXMLNode(_nsuSys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.TempTriggers));
            _triggers.Add(trg);
        }

        public override IExternalCommandResult ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            _logger.LogWarning($"ProccessExternalCommand() not implemented for 'Target:{command.Target}' and 'Action:{command.Action}'.");
            return null;
        }

        public override void Clear()
        {
            _triggers.Clear();
        }
    }
}

