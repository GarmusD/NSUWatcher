using System;
using System.Collections.Generic;
using NSU.Shared.NSUSystemPart;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.Interfaces.MCUCommands.From;
using NSUWatcher.NSUSystem.Data;
using NSUWatcher.Interfaces;
using NSU.Shared;
using NSU.Shared.Serializer;
using Microsoft.Extensions.Logging;
using System.Collections;
using NSU.Shared.DataContracts;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class RelayModules : NSUSysPartBase
    {
        readonly List<RelayModule> _modules = new List<RelayModule>();

        public override string[] SupportedTargets => new string[] { JKeys.RelayModule.TargetName };

        public RelayModules(NsuSystem sys, ILoggerFactory loggerFactory, INsuSerializer serializer) : base(sys, loggerFactory, serializer, PartType.RelayModules)
        {
        }

        public override bool ProcessCommandFromMcu(IMessageFromMcu command)
        {
            switch (command)
            {
                case IRelaySnapshot snapshot:
                    ProcessSnapshot(snapshot);
                    return true;

                case IRelayInfo status:
                    ProcessStatus(status);
                    return true;

                default:
                    return false;
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


        public override IExternalCommandResult ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            _logger.LogWarning($"ProccessExternalCommand() not implemented for 'Target:{command.Target}' and 'Action:{command.Action}'.");
            return null;
        }

        public override void Clear()
        {
            _modules.Clear();
        }

#nullable enable
        public override IEnumerator? GetEnumerator<T>()
        {
            return (typeof(T) is IRelayModuleDataContract) ? _modules.GetEnumerator() : (IEnumerator?)null;
        }
#nullable disable
    }
}

