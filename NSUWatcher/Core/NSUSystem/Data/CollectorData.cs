using NSU.Shared.DataContracts;
using NSU.Shared.NSUSystemPart;
using NSUWatcher.Interfaces.MCUCommands.From;
using System;
using System.Linq;

namespace NSUWatcher.NSUSystem.Data
{
    public class CollectorData : ICollectorDataContract
    {
        public byte ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CircPumpName { get; set; } = string.Empty;
        public int ActuatorsCount => _thermoActuatorData.Length;

        public IThermoActuatorDataContract[] Actuators => _thermoActuatorData;

        private readonly ThermoActuatorData[] _thermoActuatorData = Enumerable
            .Range(0, Collector.MaxCollectorActuators)
            .Select((n) => { return new ThermoActuatorData((byte)n); })
            .ToArray();

        public CollectorData()
        {

        }

        public CollectorData(ICollectorSnapshot snapshot)
        {
            ConfigPos = snapshot.ConfigPos;
            Enabled = snapshot.Enabled;
            Name = snapshot.Name;
            CircPumpName = snapshot.CircPumpName;
            for (int i = 0; i < snapshot.ActuatorsCounts; i++)
            {
                var actuator = _thermoActuatorData[i];
                actuator.RelayChannel = snapshot.Actuators[i].Channel;
                actuator.Type = (ActuatorType)snapshot.Actuators[i].ActuatorType;
                actuator.Opened = snapshot.Actuators[i].IsOpen.GetValueOrDefault();
            }
        }
    }

    public class ThermoActuatorData : IThermoActuatorDataContract
    {
        public byte Index { get; }

        public ActuatorType Type { get; set; }
        public byte RelayChannel { get; set; }
        public bool? Opened { get; set; }

        public ThermoActuatorData(byte index)
        {
            Index = index;
        }
    }
}
