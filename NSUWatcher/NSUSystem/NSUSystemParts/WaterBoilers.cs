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
    public class WaterBoilers : NSUSysPartBase
    {
        public override string[] SupportedTargets => new string[] { JKeys.WaterBoiler.TargetName };
        
        private readonly List<WaterBoiler> _boilers = new List<WaterBoiler>();

        public WaterBoilers(NsuSystem sys, ILoggerFactory loggerFactory, INsuSerializer serializer) : base(sys, loggerFactory, serializer, PartType.WaterBoilers) { }

        public override bool ProcessCommandFromMcu(IMessageFromMcu command)
        {
            switch (command)
            {
                case IWaterBoilerSnapshot snapshot:
                    var dataContract = new WaterBoilerDataContract(snapshot);
                    var wb = new WaterBoiler(dataContract);
                    wb.AttachXMLNode(_nsuSys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.WaterBoilers));
                    _boilers.Add(wb);
                    return true;

                default:
                    return false;
            }
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

#nullable enable
        public override IEnumerator? GetEnumerator<T>()
        {
            return (typeof(T) is IWaterBoilerDataContract) ? _boilers.GetEnumerator() : (IEnumerator?)null;
        }
#nullable disable
    }
}
