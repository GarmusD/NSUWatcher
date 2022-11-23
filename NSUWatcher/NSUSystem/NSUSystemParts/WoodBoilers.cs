using System;
using System.Collections.Generic;
using NSU.Shared.NSUSystemPart;
using Serilog;
using System.Linq;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.Interfaces.MCUCommands.From;
using NSUWatcher.NSUSystem.Data;
using NSUWatcher.Interfaces;
using NSU.Shared;
using NSU.Shared.Serializer;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{

    public class WoodBoilers : NSUSysPartBase
    {
        public override string[] SupportedTargets => new string[] { JKeys.WoodBoiler.TargetName };


        private readonly List<WoodBoiler> _boilers = new List<WoodBoiler>();

        public WoodBoilers(NsuSystem sys, ILogger logger, INsuSerializer serializer) : base(sys, logger, serializer, PartType.WoodBoilers) { }

        public WoodBoiler? FindWoodBoiler(string name)
        {
            return _boilers.FirstOrDefault(x => x.Name == name);
        }

        public override void ProcessCommandFromMcu(IMessageFromMcu command)
        {
            switch (command)
            {
                case IWoodBoilerSnapshot snapshot:
                    ProcessSnapshot(snapshot);
                    return;

                case IWoodBoilerInfo boilerInfo:
                    ProcessWoodBoilerInfo(boilerInfo);
                    return;

                case IWoodBoilerLadomatInfo ladomatInfo:
                    ProcessLadomatInfo(ladomatInfo);
                    return;

                case IWoodBoilerExhaustFanInfo exhaustFanInfo:
                    ProcessExhaustFanInfo(exhaustFanInfo);
                    return;

                default:
                    LogNotImplementedCommand(command);
                    break;
            }
        }

        private void ProcessSnapshot(IWoodBoilerSnapshot snapshot)
        {
            var dataContract = new WoodBoilerData(snapshot);
            var wb = new WoodBoiler(dataContract);
            wb.AttachXMLNode(_nsuSys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.WoodBoilers));
            _boilers.Add(wb);

            wb.PropertyChanged += (s, e) => 
            {
                if (s is WoodBoiler woodBoiler)
                    OnPropertyChanged(woodBoiler, e.PropertyName);
            };
        }

        private void ProcessWoodBoilerInfo(IWoodBoilerInfo boilerInfo)
        {
            var wb = FindWoodBoiler(boilerInfo.Name);
            if (wb != null)
            {
                wb.CurrentTemp = boilerInfo.CurrentTemperature;
                wb.WBStatus = Enum.Parse<WoodBoilerStatus>(boilerInfo.WBStatus, true);
                wb.TempStatus = Enum.Parse<WoodBoilerTempStatus>(boilerInfo.TempStatus, true);
            }
            else LogWBNotFound(boilerInfo.Name);
        }

        private void ProcessLadomatInfo(IWoodBoilerLadomatInfo ladomatInfo)
        {
            var wb = FindWoodBoiler(ladomatInfo.Name);
            if(wb != null) wb.LadomStatus = Enum.Parse<Status>(ladomatInfo.LadomatStatus, true);
        }

        private void ProcessExhaustFanInfo(IWoodBoilerExhaustFanInfo exhaustFanInfo)
        {
            var wb = FindWoodBoiler(exhaustFanInfo.Name);
            if (wb != null) wb.ExhaustFanStatus = Enum.Parse<Status>(exhaustFanInfo.ExhaustFanStatus, true);
        }

        private void LogWBNotFound(string name)
        {
            _logger.Warning($"WoodBoiler '{name}' not founded.");
        }


        public override IExternalCommandResult? ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            _logger.Warning($"ProccessExternalCommand() not implemented for 'Target:{command.Target}' and 'Action:{command.Action}'.");
            return null;
        }

        public override void Clear()
        {
            _boilers.Clear();
        }
    }
}
