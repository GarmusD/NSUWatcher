using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSU.Shared.DataContracts;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.NSUSystem.Data
{
    public class ComfortZoneData : IComfortZoneDataContract
    {
        public int ConfigPos { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string RoomSensorName { get; set; }
        public string FloorSensorName { get; set; }
        public string CollectorName { get; set; }
        public double RoomTempHi { get; set; }
        public double RoomTempLow { get; set; }
        public double FloorTempHi { get; set; }
        public double FloorTempLow { get; set; }
        public double Histeresis { get; set; }
        public int Actuator { get; set; }
        public bool LowTempMode { get; set; }
        public double CurrentRoomTemperature { get; set; }
        public double CurrentFloorTemperature { get; set; }
        public bool ActuatorOpened { get; set; }

        public ComfortZoneData(IComfortZoneSnapshot snapshot)
        {
            ConfigPos = snapshot.ConfigPos;
            Enabled = snapshot.Enabled;
            Name = snapshot.Name;
            Title = snapshot.Title;
            RoomSensorName = snapshot.RoomSensorName;
            FloorSensorName = snapshot.FloorSensorName;
            CollectorName = snapshot.CollectorName;
            RoomTempHi = snapshot.RoomTempHi;
            RoomTempLow = snapshot.RoomTempLow;
            FloorTempHi = snapshot.FloorTempHi;
            FloorTempLow = snapshot.FloorTempLow;
            Histeresis = snapshot.Histeresis;
            Actuator = snapshot.ActuatorPosition;
            LowTempMode = snapshot.LowTempMode;
            CurrentRoomTemperature = snapshot.CurrentRoomTemp.GetValueOrDefault(-127.0);
            CurrentFloorTemperature = snapshot.CurrentFloorTemp.GetValueOrDefault(-127.0);
            ActuatorOpened = snapshot.ActuatorOpened.GetValueOrDefault();
        }
    }
}