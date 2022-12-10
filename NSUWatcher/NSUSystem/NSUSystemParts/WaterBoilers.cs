using System.Collections.Generic;
using NSU.Shared.NSUSystemPart;
using Serilog;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.Interfaces.MCUCommands.From;
using NSUWatcher.NSUSystem.Data;
using NSUWatcher.Interfaces;
using System;
using NSU.Shared;
using NSU.Shared.Serializer;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class WaterBoilers : NSUSysPartBase
    {
        public override string[] SupportedTargets => new string[] { JKeys.WaterBoiler.TargetName };
        
        private readonly List<WaterBoiler> _boilers = new List<WaterBoiler>();

        public WaterBoilers(NsuSystem sys, ILogger logger, INsuSerializer serializer) : base(sys, logger, serializer, PartType.WaterBoilers) { }

        public override void ProcessCommandFromMcu(IMessageFromMcu command)
        {
            switch (command)
            {
                case IWaterBoilerSnapshot snapshot:
                    var dataContract = new WaterBoilerDataContract(snapshot);
                    var wb = new WaterBoiler(dataContract);
                    wb.AttachXMLNode(_nsuSys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.WaterBoilers));
                    _boilers.Add(wb);
                    return;

                default:
                    break;
            }
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
