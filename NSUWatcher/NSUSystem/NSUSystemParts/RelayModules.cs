using System;
using System.Collections.Generic;
using NSU.Shared.NSUSystemPart;
using Serilog;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.Interfaces.MCUCommands.From;
using NSUWatcher.NSUSystem.Data;
using NSUWatcher.Interfaces;
using NSU.Shared;
using NSU.Shared.Serializer;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class RelayModules : NSUSysPartBase
    {
        readonly List<RelayModule> _modules = new List<RelayModule>();

        public override string[] SupportedTargets => new string[] { JKeys.RelayModule.TargetName };

        public RelayModules(NsuSystem sys, ILogger logger, INsuSerializer serializer) : base(sys, logger, serializer, PartType.RelayModules)
        {
        }

        public override void ProcessCommandFromMcu(IMessageFromMcu command)
        {
            switch (command)
            {
                case IRelaySnapshot snapshot:
                    ProcessSnapshot(snapshot);
                    return;

                case IRelayInfo status:
                    ProcessStatus(status);
                    return;

                default:
                    LogNotImplementedCommand(command);
                    return;
            }
        }

        private void ProcessSnapshot(IRelaySnapshot snapshot)
        {
            var dataContract = new RelayModuleData(snapshot);
            var rm = new RelayModule(dataContract);
            rm.AttachXMLNode(_nsuSys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.RelayModules));
            _modules.Add(rm);

            rm.PropertyChanged += (s, e) =>
            {
                if (s is RelayModule relayModule)
                    OnPropertyChanged(relayModule, e.PropertyName);
            };

        }
        
        private void ProcessStatus(IRelayInfo status)
        {
            for (var i = 0; i < _modules.Count; i++)
            {
                _modules[i].SetStatus(status.Values[i]);
            }
        }


        public override IExternalCommandResult? ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            _logger.Warning($"ProccessExternalCommand() not implemented for 'Target:{command.Target}' and 'Action:{command.Action}'.");
            return null;
        }

        public override void Clear()
        {
            _modules.Clear();
        }
    }
}

