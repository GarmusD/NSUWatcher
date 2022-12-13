using System;
using System.Collections.Generic;
using NSU.Shared.NSUSystemPart;
using System.Collections;
using System.Linq;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.Interfaces.MCUCommands.From;
using NSUWatcher.Interfaces;
using NSU.Shared;
using NSU.Shared.Serializer;
using Microsoft.Extensions.Logging;
using NSU.Shared.DataContracts;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{

    public class TempSensors : NSUSysPartBase
    {
        public override string[] SupportedTargets => new string[] { JKeys.TempSensor.TargetName };
        
        private readonly List<TempSensor> _sensors = new List<TempSensor>();
        private readonly BitArray _configIDs = new BitArray(PartConsts.MAX_TEMP_SENSORS);

        public TempSensors(NsuSystem sys, ILoggerFactory loggerFactory, INsuSerializer serializer) : base(sys, loggerFactory, serializer, PartType.TSensors) {}

        public void AddSensor(TempSensor value)
        {
            if (_sensors.Count < PartConsts.MAX_TEMP_SENSORS)
            {
                _sensors.Add(value);
            }
            else
            {
                // The code should not be reached here.
                // If this happens - something wrong with hardware.
                _logger.LogError($"Too much TempSensors online! Count exeeds limit of {PartConsts.MAX_TEMP_SENSORS} sensors.");
                throw new IndexOutOfRangeException();
            }
        }

        public TempSensor FindSensor(byte[] addr)
        {
            return _sensors.Find(x => x.CompareAddr(addr));
        }

        bool IsAddrValid(byte[] addr)
        {
            return !TempSensor.IsAddressNull(addr);
        }

        private byte GetFirstFreeConfigID()
        {
            for (byte i = 0; i < _configIDs.Count; i++)
                if (!_configIDs[i])
                    return i;
            return NSUPartBase.INVALID_VALUE;
        }

        private void UpdateConfigIDs()
        {
            foreach (var sensor in _sensors)
            {
                if (sensor.ConfigPos != NSUPartBase.INVALID_VALUE)
                    _configIDs[sensor.ConfigPos] = true;
            }

            var invCfgPosList = _sensors.FindAll(x => (x.ConfigPos == NSUPartBase.INVALID_VALUE && !TempSensor.IsAddressNull(x))).ToList();
            foreach (var sensor in invCfgPosList)
            {
                byte freePos = GetFirstFreeConfigID();
                if (freePos != NSUPartBase.INVALID_VALUE)
                {
                    sensor.ConfigPos = freePos;
                    _configIDs[freePos] = true;
                    _logger.LogError($"Sensor '{TempSensor.AddrToString(sensor.SensorID)}' with invalid ConfigPos founded. New ConfigPos is {freePos}.");
                }
                else
                {
                    _logger.LogError($"No more free indexes for config is left. Cannot assign ConfigPos to a sensor '{TempSensor.AddrToString(sensor.SensorID)}'");
                }
            }
        }

        public override bool ProcessCommandFromMcu(IMessageFromMcu command)
        {
            switch (command)
            {
                case ITSensorInfo tSensorInfo:
                    ProcessInfo(tSensorInfo);
                    return true;

                case ITSensorSystemSnapshot systemSnapshot:
                    ProcessSystemSnapshot(systemSnapshot);
                    return true;

                case ITSensorConfigSnapshot configSnapshot:
                    ProcessConfigSnapshot(configSnapshot);
                    return true;

                case ISystemSnapshotDone _:
                    UpdateConfigIDs();
                    return true;

                default:
                    return false;
            }
        }

        private void ProcessSystemSnapshot(ITSensorSystemSnapshot systemSnapshot)
        {
            var ts = new TempSensor
            {
                SensorID = TempSensor.StringToAddr(systemSnapshot.Address),
                Temperature = systemSnapshot.Temperature,
                ReadErrorCount = systemSnapshot.ReadErrors
            };
            ts.AttachXMLNode(_nsuSys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.TSensors));
            AddSensor(ts);
            ts.PropertyChanged += (s, e) => { OnPropertyChanged(s as TempSensor, e.PropertyName); };
        }

        private void ProcessConfigSnapshot(ITSensorConfigSnapshot configSnapshot)
        {
            byte[] sensorAddr = TempSensor.StringToAddr(configSnapshot.Address);
            if (IsAddrValid(sensorAddr))
            {
                var ts = FindSensor(sensorAddr);
                if (ts == null)
                {
                    ts = new TempSensor();
                    ts.AttachXMLNode(_nsuSys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.TSensors));
                    if (!TempSensor.IsAddressNull(ts))
                        ts.NotFound = true;
                    AddSensor(ts);

                }
                ts.ConfigPos = configSnapshot.ConfigPos;
                ts.Enabled = configSnapshot.Enabled;
                ts.SensorID = TempSensor.StringToAddr(configSnapshot.Address);
                ts.Name = configSnapshot.Name;
                ts.Interval = configSnapshot.Interval;
            }
        }
        
        private void ProcessInfo(ITSensorInfo tSensorInfo)
        {
            var ts = FindSensor(TempSensor.StringToAddr(tSensorInfo.SensorID));
            if (ts != null)
            {
                ts.ReadErrorCount = tSensorInfo.ReadErrorCount;
                ts.Temperature = tSensorInfo.Temperature;
            }
        }

        public override IExternalCommandResult ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            _logger.LogWarning($"ProccessExternalCommand() not implemented for 'Target:{command.Target}' and 'Action:{command.Action}'.");
            return null;
        }

        public override void Clear()
        {
            _sensors.Clear();
        }

#nullable enable
        public override IEnumerable? GetEnumerator<T>()
        {
            return typeof(TempSensor).GetInterfaces().Contains(typeof(T)) ? _sensors : (IEnumerable?)null;
        }
#nullable disable
    }
}

