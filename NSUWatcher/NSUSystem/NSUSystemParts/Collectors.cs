﻿using System.Linq;
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
    public class Collectors : NSUSysPartBase
    {
        public override string[] SupportedTargets => new string[] { JKeys.Collector.TargetName, "COLLECTOR:" };
        
        private readonly List<Collector> _collectors = new List<Collector>();

        public Collectors(NsuSystem sys, ILogger logger, INsuSerializer serializer) : base(sys, logger, serializer, PartType.Collectors)
        {
        }

        private Collector? FindCollector(string name)
        {
            return _collectors.FirstOrDefault(x => x.Name == name);
        }


        /// <summary>
        /// Entry point for processing a messages from the MCU.
        /// </summary>
        /// <param name="command">Message to process</param>
        public override void ProcessCommandFromMcu(IMessageFromMcu command)
        {
            switch (command)
            {
                case ICollectorSnapshot collectorSnapshot:
                    ProcessCollectorSnapshot(collectorSnapshot);
                    return;

                case ICollectorInfo collectorInfo:
                    ProcessCollectorInfo(collectorInfo);
                    return;

                default:
                    LogNotImplementedCommand(command);
                    break;
            }
        }

        private void ProcessCollectorSnapshot(ICollectorSnapshot collectorSnapshot)
        {
            var dataContract = new CollectorData(collectorSnapshot);
            Collector cl = new Collector(dataContract);
            cl.AttachXMLNode(_nsuSys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.Collectors));
            _collectors.Add(cl);

            cl.PropertyChanged += (s, e) => 
            { 
                if(s is Collector collector)
                    OnPropertyChanged(collector, e.PropertyName);
            };
        }
        
        private void ProcessCollectorInfo(ICollectorInfo collectorInfo)
        {
            var col = FindCollector(collectorInfo.Name);
            col?.UpdateActuatorStatus(collectorInfo.OpenedValves);
        }


        public override IExternalCommandResult? ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            _logger.Warning($"ProccessExternalCommand() not implemented for 'Target:{command.Target}' and 'Action:{command.Action}'.");
            return null;
        }

        public override void Clear()
        {
            _collectors.Clear();
        }
    }
}

