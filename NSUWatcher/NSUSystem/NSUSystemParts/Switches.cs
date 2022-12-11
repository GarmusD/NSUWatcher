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
    public class Switches : NSUSysPartBase
    {
        public override string[] SupportedTargets => new string[] { JKeys.Switch.TargetName, "SWITCH:" };
        
        private readonly List<Switch> _switches = new List<Switch>();

        public Switches(NsuSystem sys, ILoggerFactory loggerFactory, INsuSerializer serializer) : base(sys, loggerFactory, serializer, PartType.Switches) {}


        private Switch FindSwitch(string name)
        {
            return _switches.FirstOrDefault(x => x.Name == name);
        }

        private Switch FindSwitch(byte cfgPos)
        {
            return _switches.FirstOrDefault(x => x.ConfigPos == cfgPos);
        }

        public override bool ProcessCommandFromMcu(IMessageFromMcu command)
        {
            switch (command)
            {
                case ISwitchSnapshot snapshot:
                    ProcessSnapshot(snapshot);
                    return true;

                case ISwitchInfo switchInfo:
                    ProcessInfo(switchInfo);
                    return true;

                default:
                    return false;
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
                sw.Status = (Status)Enum.Parse(typeof(Status), switchInfo.Status, true);
            }
        }

        public override IExternalCommandResult ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            _logger.LogWarning($"ProccessExternalCommand() not implemented for 'Target:{command.Target}' and 'Action:{command.Action}'.");
            return null;
        }

        public override void Clear()
        {
            _switches.Clear();
        }

        private void Switch_OnClicked(object sender, EventArgs e)
        {
            _logger.LogDebug($"Switch_OnClicked(). Name: {(sender as Switch)?.Name}. Sending to Arduino.");
        }

        
    }
}

