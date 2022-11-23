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
    public class KTypes : NSUSysPartBase
    {
        readonly List<KType> _ktypes = new List<KType>();

        public override string[] SupportedTargets => new string[] { JKeys.KType.TargetName };

        public KTypes(NsuSystem sys, ILogger logger, INsuSerializer serializer) : base(sys, logger, serializer, PartType.KTypes) { }

        public KType? FindKType(string name)
        {
            return _ktypes.FirstOrDefault(x => x.Name == name);
        }

        public override void ProcessCommandFromMcu(IMessageFromMcu command)
        {
            switch (command)
            {
                case IKTypeSnapshot snapshot:
                    ProcessActionSnapshot(snapshot);
                    return;

                case IKTypeInfo kTypeInfo:
                    ProcessActionInfo(kTypeInfo);
                    return;

                default:
                    LogNotImplementedCommand(command);
                    break;
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

        public override IExternalCommandResult? ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            _logger.Warning($"ProccessExternalCommand() not implemented for 'Target:{command.Target}' and 'Action:{command.Action}'.");
            return null;
        }

        public override void Clear()
        {
            _ktypes.Clear();
        }
    }
}
