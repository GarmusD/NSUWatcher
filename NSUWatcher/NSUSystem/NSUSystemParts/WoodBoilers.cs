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

    public class WoodBoilers : NSUSysPartBase
    {
        public override string[] SupportedTargets => new string[] { JKeys.WoodBoiler.TargetName };


        private readonly List<WoodBoiler> _boilers = new List<WoodBoiler>();

        public WoodBoilers(NsuSystem sys, ILoggerFactory loggerFactory, INsuSerializer serializer) : base(sys, loggerFactory, serializer, PartType.WoodBoilers) { }

        public WoodBoiler FindWoodBoiler(string name)
        {
            return _boilers.FirstOrDefault(x => x.Name == name);
        }

        public override bool ProcessCommandFromMcu(IMessageFromMcu command)
        {
            switch (command)
            {
                case IWoodBoilerSnapshot snapshot:
                    ProcessSnapshot(snapshot);
                    return true;

                case IWoodBoilerInfo boilerInfo:
                    ProcessWoodBoilerInfo(boilerInfo);
                    return true;

                case IWoodBoilerLadomatInfo ladomatInfo:
                    ProcessLadomatInfo(ladomatInfo);
                    return true;

                case IWoodBoilerExhaustFanInfo exhaustFanInfo:
                    ProcessExhaustFanInfo(exhaustFanInfo);
                    return true;

                default:
                    return false;
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
                wb.WBStatus =  (WoodBoilerStatus)Enum.Parse(typeof(WoodBoilerStatus), boilerInfo.WBStatus, true);
                wb.TempStatus = (WoodBoilerTempStatus)Enum.Parse(typeof(WoodBoilerTempStatus), boilerInfo.TempStatus, true);
            }
            else LogWBNotFound(boilerInfo.Name);
        }

        private void ProcessLadomatInfo(IWoodBoilerLadomatInfo ladomatInfo)
        {
            var wb = FindWoodBoiler(ladomatInfo.Name);
            if(wb != null) wb.LadomStatus = (Status)Enum.Parse(typeof(Status), ladomatInfo.LadomatStatus, true);
        }

        private void ProcessExhaustFanInfo(IWoodBoilerExhaustFanInfo exhaustFanInfo)
        {
            var wb = FindWoodBoiler(exhaustFanInfo.Name);
            if (wb != null) wb.ExhaustFanStatus = (Status)Enum.Parse(typeof(Status), exhaustFanInfo.ExhaustFanStatus, true);
        }

        private void LogWBNotFound(string name)
        {
            _logger.LogWarning($"WoodBoiler '{name}' not founded.");
        }


        public override IExternalCommandResult ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            _logger.LogWarning($"ProccessExternalCommand() not implemented for 'Target:{command.Target}' and 'Action:{command.Action}'.");
            return null;
        }

        public override void Clear()
        {
            _boilers.Clear();
        }
    }
}
