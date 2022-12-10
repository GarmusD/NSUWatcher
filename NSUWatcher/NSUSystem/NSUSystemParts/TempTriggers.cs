using System;
using System.Collections.Generic;
using NSU.Shared.NSUSystemPart;
using System.Linq;
using Serilog;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.Interfaces.MCUCommands.From;
using NSUWatcher.NSUSystem.Data;
using NSUWatcher.Interfaces;
using NSU.Shared;
using NSU.Shared.Serializer;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class TempTriggers : NSUSysPartBase
    {
        public override string[] SupportedTargets => new string[] { JKeys.TempTrigger.TargetName, "TRIGGER:" };

        readonly List<TempTrigger> _triggers = new List<TempTrigger>();

        public TempTriggers(NsuSystem sys, ILogger logger, INsuSerializer serializer) : base(sys, logger, serializer, PartType.TempTriggers) { }

        private TempTrigger FindTrigger(string name)
        {
            return _triggers.FirstOrDefault(x => x.Name == name);
        }

        public override void ProcessCommandFromMcu(IMessageFromMcu command)
        {
            switch (command)
            {
                case ITempTriggerSnapshot snapshot:
                    ProcessSnapshot(snapshot);
                    return;

                case ITempTriggerInfo triggerInfo:
                    ProcessInfo(triggerInfo);
                    return;

                default:
                    LogNotImplementedCommand(command);
                    break;
            }
        }

        private void ProcessInfo(ITempTriggerInfo triggerInfo)
        {
            var trg = FindTrigger(triggerInfo.Name);
            if (trg != null)
            {
                trg.Status = Enum.Parse<Status>(triggerInfo.Status, true);
            }
        }

        private void ProcessSnapshot(ITempTriggerSnapshot snapshot)
        {
            var dataContract = new TempTriggerData(snapshot);
            var trg = new TempTrigger(dataContract);
            trg.AttachXMLNode(_nsuSys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.TempTriggers));
            _triggers.Add(trg);
        }

        public override IExternalCommandResult? ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            _logger.Warning($"ProccessExternalCommand() not implemented for 'Target:{command.Target}' and 'Action:{command.Action}'.");
            return null;
        }

        public override void Clear()
        {
            _triggers.Clear();
        }
    }
}

