using System;
using NSU.Shared.NSUTypes;
using System.Collections.Generic;
using NSU.Shared.NSUSystemPart;
using Newtonsoft.Json.Linq;
using NSU.Shared;
using NSUWatcher.NSUWatcherNet;
using System.Linq;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class ComfortZones : NSUSysPartsBase
    {
        readonly string LogTag = "ComfortZones";
        private readonly List<ComfortZone> _comfZones = new List<ComfortZone>();

        public ComfortZones(NSUSys sys, PartsTypes type)
            : base(sys, type)
        {
        }

        public override string[] RegisterTargets()
        {
            return new string[] { JKeys.ComfortZone.TargetName, "CZONE:" };
        }

        public ComfortZone FindComfortZone(int idx)
        {
            return _comfZones.FirstOrDefault(x => x.ConfigPos == idx);
        }

        public ComfortZone FindComfortZone(string name)
        {
            return _comfZones.FirstOrDefault(x => x.Name == name);
        }

        public override void ProccessArduinoData(JObject data)
        {
            switch ((string)data[JKeys.Generic.Action])
            {
                case JKeys.Syscmd.Snapshot:
                    ProcessActionSnapshot(data);
                    break;
                case JKeys.Action.Info:
                    ProcessActionInfo(data);
                    break;
            }
        }

        private void ProcessActionSnapshot(JObject data)
        {
            if (data.Property(JKeys.Generic.Result) == null)
            {
                ComfortZone czn = new ComfortZone();
                czn.ConfigPos = JSonValueOrDefault(data, JKeys.Generic.ConfigPos, ComfortZone.INVALID_VALUE);
                czn.Enabled = Convert.ToBoolean(JSonValueOrDefault(data, JKeys.Generic.Enabled, (byte)0));
                czn.Name = JSonValueOrDefault(data, JKeys.Generic.Name, string.Empty);
                czn.Title = JSonValueOrDefault(data, JKeys.ComfortZone.Title, string.Empty);
                czn.CollectorName = JSonValueOrDefault(data, JKeys.ComfortZone.CollectorName, string.Empty);
                czn.Actuator = JSonValueOrDefault(data, JKeys.ComfortZone.Actuator, ComfortZone.INVALID_VALUE);
                czn.Histeresis = JSonValueOrDefault(data, JKeys.ComfortZone.Histeresis, 0);
                czn.RoomSensorName = JSonValueOrDefault(data, JKeys.ComfortZone.RoomSensorName, string.Empty);
                czn.RoomTempHi = JSonValueOrDefault(data, JKeys.ComfortZone.RoomTempHi, 0f);
                czn.RoomTempLow = JSonValueOrDefault(data, JKeys.ComfortZone.RoomTempLow, 0f);
                czn.FloorSensorName = JSonValueOrDefault(data, JKeys.ComfortZone.FloorSensorName, string.Empty);
                czn.FloorTempHi = JSonValueOrDefault(data, JKeys.ComfortZone.FloorTempHi, 0f);
                czn.FloorTempLow = JSonValueOrDefault(data, JKeys.ComfortZone.FloorTempLow, 0f);

                if (JSonValueOrDefault(data, JKeys.Generic.Content, JKeys.Content.Config) == JKeys.Content.ConfigPlus)
                {
                    czn.CurrentRoomTemperature = JSonValueOrDefault(data, JKeys.ComfortZone.CurrentRoomTemp, 0f);
                    czn.CurrentFloorTemperature = JSonValueOrDefault(data, JKeys.ComfortZone.CurrentFloorTemp, 0f);
                    czn.ActuatorOpened = JSonValueOrDefault(data, JKeys.ComfortZone.ActuatorOpened, false);
                }
                czn.AttachXMLNode(nsusys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.ComfortZones));
                czn.OnFloorTemperatureChanged += OnFloorTemperatureChangedHandler;
                czn.OnRoomTemperatureChanged += OnRoomTemperatureChangedHandler;
                czn.OnActuatorOpenedChanged += OnActuatorOpenedChangedHandler;
                _comfZones.Add(czn);
            }
        }

        private void ProcessActionInfo(JObject data)
        {
            string name = (string)data[JKeys.Generic.Name];
            var cz = FindComfortZone(name);
            if (cz != null)
            {
                switch ((string)data[JKeys.Generic.Content])
                {
                    case JKeys.ComfortZone.CurrentRoomTemp:
                        cz.CurrentRoomTemperature = (float)data[JKeys.Generic.Value];
                        break;
                    case JKeys.ComfortZone.CurrentFloorTemp:
                        cz.CurrentFloorTemperature = (float)data[JKeys.Generic.Value];
                        break;
                    case JKeys.ComfortZone.ActuatorOpened:
                        cz.ActuatorOpened = (bool)data[JKeys.Generic.Value];
                        break;
                }
            }
        }

        public override void ProccessNetworkData(NetClientData clientData, JObject data)
        {
            switch ((string)data[JKeys.Generic.Action])
            {
                case JKeys.Action.Get:
                    return;
                case JKeys.Action.Set:
                    //check user access
                    return;
            }
        }

        private void OnActuatorOpenedChangedHandler(object sender, ActuatorOpenedEventArgs e)
        {
            NSULog.Debug(LogTag, "OnValveOpenedChangedHandler(). Creating Jobject()");
            JObject jo = new JObject
            {
                [JKeys.Generic.Target] = JKeys.ComfortZone.TargetName,
                [JKeys.Generic.Action] = JKeys.Action.Info,
                [JKeys.Generic.Name] = (sender as ComfortZone).Name,
                [JKeys.Generic.Content] = JKeys.ComfortZone.ActuatorOpened,
                [JKeys.Generic.Value] = e.Opened
            };
            SendToClient(NetClientRequirements.CreateStandartAcceptInfo(), jo);
        }

        private void OnRoomTemperatureChangedHandler(object sender, TempChangedEventArgs e)
        {
            NSULog.Debug(LogTag, "OnRoomTemperatureChangedHandler(). Creating Jobject()");
            JObject jo = new JObject
            {
                [JKeys.Generic.Target] = JKeys.ComfortZone.TargetName,
                [JKeys.Generic.Action] = JKeys.Action.Info,
                [JKeys.Generic.Name] = (sender as ComfortZone).Name,
                [JKeys.Generic.Content] = JKeys.ComfortZone.CurrentRoomTemp,
                [JKeys.Generic.Value] = e.Temperature
            };
            SendToClient(NetClientRequirements.CreateStandartAcceptInfo(), jo);
        }

        private void OnFloorTemperatureChangedHandler(object sender, TempChangedEventArgs e)
        {
            NSULog.Debug(LogTag, "OnFloorTemperatureChangedHandler(). Creating Jobject()");
            JObject jo = new JObject
            {
                [JKeys.Generic.Target] = JKeys.ComfortZone.TargetName,
                [JKeys.Generic.Action] = JKeys.Action.Info,
                [JKeys.Generic.Name] = (sender as ComfortZone).Name,
                [JKeys.Generic.Content] = JKeys.ComfortZone.CurrentFloorTemp,
                [JKeys.Generic.Value] = e.Temperature
            };
            SendToClient(NetClientRequirements.CreateStandartAcceptInfo(), jo);
        }

        public override void Clear()
        {
            _comfZones.Clear();
        }
    }
}
