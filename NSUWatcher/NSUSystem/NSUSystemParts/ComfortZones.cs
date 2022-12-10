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
    public class ComfortZones : NSUSysPartBase
    {
        public override string[] SupportedTargets => new string[] { JKeys.ComfortZone.TargetName, "CZONE:" };

        private readonly List<ComfortZone> _comfZones = new List<ComfortZone>();

        public ComfortZones(NsuSystem sys, ILoggerFactory loggerFactory, INsuSerializer serializer) : base(sys, loggerFactory, serializer, PartType.ComfortZones) { }

        public ComfortZone FindComfortZone(int idx)
        {
            return _comfZones.FirstOrDefault(x => x.ConfigPos == idx);
        }

        public ComfortZone FindComfortZone(string name)
        {
            return _comfZones.FirstOrDefault(x => x.Name == name);
        }

        public override bool ProcessCommandFromMcu(IMessageFromMcu command)
        {
            switch (command)
            {
                case IComfortZoneSnapshot snapshot:
                    ProcessActionSnapshot(snapshot);
                    return true;

                case IComfortZoneRoomTempInfo roomTempInfo:
                    ProcessCurrentRoomTemp(roomTempInfo);
                    return true;

                case IComfortZoneFloorTempInfo floorTempInfo:
                    ProcessCurrentFloorTemp(floorTempInfo);
                    return true;

                case IComfortZoneActuatorStatus actuatorStatus:
                    ProcessActuatorStatus(actuatorStatus);
                    return true;

                case IComfortZoneLowTempMode lowTempMode:
                    ProcessLowTempMode(lowTempMode);
                    return true;

                default:
                    return false;
            }
        }

        private void ProcessCurrentRoomTemp(IComfortZoneRoomTempInfo roomTempInfo)
        {
            var cz = FindComfortZone(roomTempInfo.Name);
            if (cz != null) cz.CurrentRoomTemperature = roomTempInfo.Value;
        }

        private void ProcessCurrentFloorTemp(IComfortZoneFloorTempInfo floorTempInfo)
        {
            var cz = FindComfortZone(floorTempInfo.Name);
            if (cz != null) cz.CurrentFloorTemperature = floorTempInfo.Value;
        }
        
        private void ProcessActuatorStatus(IComfortZoneActuatorStatus actuatorStatus)
        {
            var cz = FindComfortZone(actuatorStatus.Name);
            if (cz != null) cz.ActuatorOpened = actuatorStatus.Value;
        }
        
        private void ProcessLowTempMode(IComfortZoneLowTempMode lowTempMode)
        {
            var cz = FindComfortZone(lowTempMode.Name);
            if (cz != null) cz.LowTempMode = lowTempMode.Value;
        }

        private void ProcessActionSnapshot(IComfortZoneSnapshot snapshot)
        {
            var dataContract = new ComfortZoneData(snapshot);
            ComfortZone czn = new ComfortZone(dataContract);
            czn.AttachXMLNode(_nsuSys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.ComfortZones));
            _comfZones.Add(czn);

            czn.PropertyChanged += (s, e) => 
            {
                if (s is ComfortZone comfortZone)
                    OnPropertyChanged(comfortZone, e.PropertyName);
            };
        }

        public override IExternalCommandResult ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            _logger.LogWarning($"ProccessExternalCommand() not implemented for 'Target:{command.Target}' and 'Action:{command.Action}'.");
            return null;
        }


        public override void Clear()
        {
            _comfZones.Clear();
        }
    }
}
