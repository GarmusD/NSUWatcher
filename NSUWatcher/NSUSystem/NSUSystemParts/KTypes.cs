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
using NSU.Shared.DataContracts;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class KTypes : NSUSysPartBase
    {
        readonly List<KType> _ktypes = new List<KType>();

        public override string[] SupportedTargets => new string[] { JKeys.KType.TargetName };

        public KTypes(NsuSystem sys, ILoggerFactory loggerFactory, INsuSerializer serializer) : base(sys, loggerFactory, serializer, PartType.KTypes) { }

        public KType FindKType(string name)
        {
            return _ktypes.FirstOrDefault(x => x.Name == name);
        }

        public override bool ProcessCommandFromMcu(IMessageFromMcu command)
        {
            switch (command)
            {
                case IKTypeSnapshot snapshot:
                    ProcessActionSnapshot(snapshot);
                    return true;

                case IKTypeInfo kTypeInfo:
                    ProcessActionInfo(kTypeInfo);
                    return true;

                default:
                    return false;
            }
        }      

        private void ProcessActionSnapshot(IKTypeSnapshot snapshot)
        {
            var dataContract = new KTypeData(snapshot);
            var ktp = new KType(dataContract);
            ktp.AttachXMLNode(_nsuSys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.KTypes));
            _ktypes.Add(ktp);
        }
        
        private void ProcessActionInfo(IKTypeInfo kTypeInfo)
        {
            var ktp = FindKType(kTypeInfo.Name);
            if (ktp != null) ktp.Temperature = Convert.ToInt32(kTypeInfo.Value);
        }

        public override IExternalCommandResult ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            _logger.LogWarning($"ProccessExternalCommand() not implemented for 'Target:{command.Target}' and 'Action:{command.Action}'.");
            return null;
        }

        public override void Clear()
        {
            _ktypes.Clear();
        }

#nullable enable
        public override IEnumerable? GetEnumerator<T>()
        {
            return (typeof(T) is IKTypeDataContract) ? _ktypes : (IEnumerable?)null;
        }
#nullable disable
    }
}
