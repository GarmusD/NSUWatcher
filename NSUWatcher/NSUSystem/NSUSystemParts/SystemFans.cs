using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NSU.Shared;
using NSU.Shared.NSUSystemPart;
using NSU.Shared.Serializer;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.Interfaces.MCUCommands.From;
using NSUWatcher.Interfaces.NsuUsers;
using NSUWatcher.NSUSystem.Data;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class SystemFans : NSUSysPartBase
    {
        public override string[] SupportedTargets => new string[] { JKeys.SystemFan.TargetName };
        
        private readonly List<SystemFan> _systemFans = new List<SystemFan>();

        public SystemFans(NsuSystem sys, ILoggerFactory loggerFactory, INsuSerializer serializer) : base(sys, loggerFactory, serializer, PartType.SystemFan) {}

        private SystemFan FindSystemFan(string name)
        {
            return _systemFans.FirstOrDefault(f => f.Name == name);
        }

        public override bool ProcessCommandFromMcu(IMessageFromMcu command)
        {
            switch (command)
            {
                case ISystemFanSnapshot snapshot:
                    ProcessSnapshot(snapshot);
                    return true;

                case ISystemFanInfo fanInfo:
                    ProcessInfo(fanInfo);
                    return true;

                default:
                    return false;
            }
        }

        private void ProcessInfo(ISystemFanInfo fanInfo)
        {
            var fan = FindSystemFan(fanInfo.Name);
            if (fan != null) fan.CurrentPWM = fan.CurrentPWM;
            else _logger.LogWarning($"SystemFan with name '{fanInfo.Name}' not founded.");
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

        public override IExternalCommandResult ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            _logger.LogWarning($"ProccessExternalCommand() not implemented for 'Target:{command.Target}' and 'Action:{command.Action}'.");
            return null;
        }

        public override void Clear()
        {
            _systemFans.Clear();
        }

#nullable enable
        public override IEnumerable? GetEnumerator<T>()
        {
            return typeof(SystemFan).GetInterfaces().Contains(typeof(T)) ? _systemFans : (IEnumerable?)null;
        }
#nullable disable
    }
}
