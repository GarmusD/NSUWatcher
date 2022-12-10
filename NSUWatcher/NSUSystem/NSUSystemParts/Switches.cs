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
    public class Switches : NSUSysPartBase
    {
        public override string[] SupportedTargets => new string[] { JKeys.Switch.TargetName, "SWITCH:" };
        
        private readonly List<Switch> _switches = new List<Switch>();

        public Switches(NsuSystem sys, ILogger logger, INsuSerializer serializer) : base(sys, logger, serializer, PartType.Switches) {}


        private Switch FindSwitch(string name)
        {
            return _switches.FirstOrDefault(x => x.Name == name);
        }

        private Switch FindSwitch(byte cfgPos)
        {
            return _switches.FirstOrDefault(x => x.ConfigPos == cfgPos);
        }

        public override void ProcessCommandFromMcu(IMessageFromMcu command)
        {
            switch (command)
            {
                case ISwitchSnapshot snapshot:
                    ProcessSnapshot(snapshot);
                    return;

                case ISwitchInfo switchInfo:
                    ProcessInfo(switchInfo);
                    return;

                default:
                    LogNotImplementedCommand(command);
                    break;
            }
        }

        private void ProcessSnapshot(ISwitchSnapshot snapshot)
        {
            var dataContract = new SwitchData(snapshot);
            var sw = new Switch(dataContract);
            sw.AttachXMLNode(_nsuSys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.Switches));
            sw.Clicked += Switch_OnClicked;
            _switches.Add(sw);
        }
        
        private void ProcessInfo(ISwitchInfo switchInfo)
        {
            var sw = FindSwitch(switchInfo.Name);
            if (sw != null)
            {
                //Set isForced first because of OnStatusChange event, which sends unchecked isForced
                sw.IsForced = switchInfo.IsForced;
                sw.Status = Enum.Parse<Status>(switchInfo.Status, true);
            }
        }

        public override IExternalCommandResult? ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            _logger.Warning($"ProccessExternalCommand() not implemented for 'Target:{command.Target}' and 'Action:{command.Action}'.");
            return null;
        }

        public override void Clear()
        {
            _switches.Clear();
        }

        private void Switch_OnClicked(object? sender, EventArgs e)
        {
            _logger.Debug($"Switch_OnClicked(). Name: {(sender as Switch)?.Name}. Sending to Arduino.");
        }

        
    }
}

