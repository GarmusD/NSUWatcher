﻿using System;
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
    public class CircPumps : NSUSysPartBase
    {
        private readonly List<CircPump> _circPumps = new List<CircPump>();

        public override string[] SupportedTargets => new string[] { JKeys.CircPump.TargetName, "CIRCPUMP:" };

        public CircPumps(NsuSystem sys, ILogger logger, INsuSerializer serializer) : base(sys, logger, serializer, PartType.CircPumps) { }

        public CircPump? FindCircPump(string name)
        {
            return _circPumps.FirstOrDefault(x => x.Name == name);
        }

        // Direction - from MCU
        public override void ProcessCommandFromMcu(IMessageFromMcu command)
        {
            switch (command)
            {
                case ICircPumpSnapshot circPumpSnapshot:
                    ProcessCircPumpSnapshot(circPumpSnapshot);
                    return;

                case ICircPumpInfo circPumpInfo:
                    ProcessCircPumpInfo(circPumpInfo);
                    return;

                default:
                    LogNotImplementedCommand(command);
                    break;
            }
        }

        private void ProcessCircPumpSnapshot(ICircPumpSnapshot snapshot)
        {
            var dataContract = new CircPumpData(snapshot);
            CircPump cp = new CircPump(dataContract);
            cp.AttachXMLNode(_nsuSys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.CirculationPumps));
            cp.PropertyChanged += (s, e) => 
            {
                if (s is CircPump circPump)
                    OnPropertyChanged(circPump, e.PropertyName);
            };
            cp.Clicked += CP_Clicked;
            _circPumps.Add(cp);
        }
        
        private void ProcessCircPumpInfo(ICircPumpInfo circPumpInfo)
        {
            var cp = FindCircPump(circPumpInfo.Name);
            if (cp != null)
            {
                cp.CurrentSpeed = circPumpInfo.CurrentSpeed;
                cp.OpenedValvesCount = circPumpInfo.ValvesOpened;
                cp.Status = Enum.Parse<Status>(circPumpInfo.Status, true);
            }
        }

        private void CP_StatusChanged(object? sender, StatusChangedEventArgs e)
        {
            if (sender is CircPump cp)
                OnPropertyChanged(cp, nameof(cp.Status));
        }

        // Direction - to MCU
        public override IExternalCommandResult? ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            if(command.Action == JKeys.Action.Click)
            {
                string cpName = command.Content;
                var cp = FindCircPump(cpName);
                if(cp == null)
                {
                    return ExtCommandResult.Failure(JKeys.CircPump.TargetName, JKeys.Action.Click, $"Invalid CircPump name '{cpName}'.");
                }
                cp.OnClicked();
                return null;
            }
            _logger.Warning($"ProccessExternalCommand() not implemented for 'Target:{command.Target}' and 'Action:{command.Action}'.");
            return null;
        }

        private void CP_Clicked(object? sender, EventArgs e)
        {
            if (sender is CircPump cp)
            {
                _logger.Debug($"CP_OnClicked(). Name: {cp.Name}. Sending to Arduino.");
                _nsuSys.CmdCenter.MCUCommands.ToMcu.CircPumpCommands.Clicked(cp.Name).Send();
            }
            else
                _logger.Warning("CP_Clicked(): sender is not CircPump. sender is '{senderType}'.", sender?.GetType());
        }


        public override void Clear()
        {
            _circPumps.Clear();
        }
    }
}

