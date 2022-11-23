using System;
using System.Collections.Generic;
using System.Linq;
using NSU.Shared;
using NSU.Shared.NSUSystemPart;
using NSU.Shared.Serializer;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.Interfaces.MCUCommands.From;
using NSUWatcher.NSUSystem.Data;
using Serilog;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class SystemFans : NSUSysPartBase
    {
        public override string[] SupportedTargets => new string[] { JKeys.SystemFan.TargetName };
        
        private readonly List<SystemFan> _systemFans = new List<SystemFan>();

        public SystemFans(NsuSystem sys, ILogger logger, INsuSerializer serializer) : base(sys, logger, serializer, PartType.SystemFan) {}

        private SystemFan? FindSystemFan(string name)
        {
            return _systemFans.FirstOrDefault(f => f.Name == name);
        }

        public override void ProcessCommandFromMcu(IMessageFromMcu command)
        {
            switch (command)
            {
                case ISystemFanSnapshot snapshot:
                    ProcessSnapshot(snapshot);
                    return;

                case ISystemFanInfo fanInfo:
                    ProcessInfo(fanInfo);
                    return;

                default:
                    LogNotImplementedCommand(command);
                    break;
            }
        }

        private void ProcessInfo(ISystemFanInfo fanInfo)
        {
            var fan = FindSystemFan(fanInfo.Name);
            if (fan != null) fan.CurrentPWM = fan.CurrentPWM;
            else _logger.Warning($"SystemFan with name '{fanInfo.Name}' not founded.");
        }

        private void ProcessSnapshot(ISystemFanSnapshot snapshot)
        {
            var dataContract = new SystemFanData(snapshot);
            SystemFan sf = new SystemFan(dataContract);
            _systemFans.Add(sf);
            sf.PropertyChanged += (s, e) =>
            { 
                if(s is SystemFan systemFan)
                {
                    OnPropertyChanged(systemFan, e.PropertyName);
                }
            };
        }

        public override IExternalCommandResult? ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            _logger.Warning($"ProccessExternalCommand() not implemented for 'Target:{command.Target}' and 'Action:{command.Action}'.");
            return null;
        }

        public override void Clear()
        {
            _systemFans.Clear();
        }
    }
}
