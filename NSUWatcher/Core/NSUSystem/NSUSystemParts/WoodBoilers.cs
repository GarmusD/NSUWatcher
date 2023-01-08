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
using System.Collections;
using NSUWatcher.Interfaces.NsuUsers;
using NSU.Shared.DTO.ExtCommandContent;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
#nullable enable
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


        public override IExternalCommandResult? ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            return command.Action switch 
            { 
                JKeys.WoodBoiler.ActionIkurimas => ProcessExtCommandIkurimas(command, nsuUser, context),
                JKeys.WoodBoiler.ActionSwitch => ProcessExtCommandSwitchManualMode(command, nsuUser, context),
                _ => ProcessExtCommandNotImplemented(command)
            };
        }

        private IExternalCommandResult? ProcessExtCommandIkurimas(IExternalCommand command, INsuUser nsuUser, object context)
        {
            WoodBoilerStartUpContent? content = _serializer.Deserialize<WoodBoilerStartUpContent>(command.Content);
            if(content == null) 
            { // TODO Error handling
                return null;
            }
            // TODO Check user permissions
            _nsuSys.CmdCenter.MCUCommands.ToMcu.WoodBoilerCommands.ActionIkurimas(content.Value.Name).Send();
            return null;
        }

        private IExternalCommandResult? ProcessExtCommandSwitchManualMode(IExternalCommand command, INsuUser nsuUser, object context)
        {
            WoodBoilerSwitchManualMode? tmpContent = _serializer.Deserialize<WoodBoilerSwitchManualMode>(command.Content);
            if (tmpContent == null)
            { // TODO Error handling
                return null;
            }
            // TODO Check user permissions
            WoodBoilerSwitchManualMode content = tmpContent.Value;
            if(content.SwitchTarget == JKeys.WoodBoiler.TargetLadomat)
                _nsuSys.CmdCenter.MCUCommands.ToMcu.WoodBoilerCommands.LadomatSwitchManualMode(content.WoodBoilerName).Send();
            else if(content.SwitchTarget == JKeys.WoodBoiler.TargetExhaustFan)
                _nsuSys.CmdCenter.MCUCommands.ToMcu.WoodBoilerCommands.ExhaustFanSwitchManualMode(content.WoodBoilerName).Send();
            return null;
        }

        private IExternalCommandResult? ProcessExtCommandNotImplemented(IExternalCommand command)
        {
            _logger.LogWarning($"ProccessExternalCommand(): NotImplemented for Target '{command.Target}' and Action '{command.Action}'.");
            return null;
        }

        public override void Clear()
        {
            _boilers.Clear();
        }

#nullable enable
        public override IEnumerable? GetEnumerator<T>()
        {
            return typeof(WoodBoiler).GetInterfaces().Contains(typeof(T)) ? _boilers : (IEnumerable?)null;
        }
#nullable disable
    }
}
