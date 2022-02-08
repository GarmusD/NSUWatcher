using System;
using NSU.Shared.NSUTypes;
using System.Collections.Generic;
using NSU.Shared.NSUSystemPart;
using Newtonsoft.Json.Linq;
using NSU.Shared;
using NSUWatcher.NSUWatcherNet;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class ComfortZones : NSUSysPartsBase
    {
        readonly string LogTag = "ComfortZones";

        List<ComfortZone> ComfZones = new List<ComfortZone>();

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
            for (int i = 0; i < ComfZones.Count; i++)
            {
                if (ComfZones[i].ConfigPos == idx) return ComfZones[i];
            }
            return null;
        }

        public ComfortZone FindComfortZone(string name)
        {
            for (int i = 0; i < ComfZones.Count; i++)
            {
                if (ComfZones[i].Name.Equals(name)) return ComfZones[i];
            }
            return null;
        }

        public override void ProccessArduinoData(JObject data)
        {
            switch ((string)data[JKeys.Generic.Action])
            {
                case JKeys.Syscmd.Snapshot:
                    if (data.Property(JKeys.Generic.Result) == null)
                    {
                        ComfortZone czn = new ComfortZone();
                        czn.ConfigPos = (int)data[JKeys.Generic.ConfigPos];
                        czn.Name = (string)data[JKeys.Generic.Name];
                        czn.Title = (string)data[JKeys.ComfortZone.Title];
                        czn.CollectorName = (string)data[JKeys.ComfortZone.CollectorName];
                        czn.Channel = (byte)data[JKeys.ComfortZone.Channel];
                        czn.Histeresis = (float)data[JKeys.ComfortZone.Histeresis];
                        czn.RoomSensorName = (string)data[JKeys.ComfortZone.RoomSensorName];
                        czn.RoomTempHi = (float)data[JKeys.ComfortZone.RoomTempHi];
                        czn.RoomTempLow = (float)data[JKeys.ComfortZone.RoomTempLow];
                        czn.FloorSensorName = (string)data[JKeys.ComfortZone.FloorSensorName];
                        czn.FloorTempHi = (float)data[JKeys.ComfortZone.FloorTempHi];
                        czn.FloorTempLow = (float)data[JKeys.ComfortZone.FloorTempLow];
                        if (((string)data[JKeys.Generic.Content]).Equals(JKeys.Content.ConfigPlus))
                        {
                            czn.CurrentRoomTemperature = (float)data[JKeys.ComfortZone.CurrentRoomTemp];
                            czn.CurrentFloorTemperature = (float)data[JKeys.ComfortZone.CurrentFloorTemp];
                            czn.ValveOpened = Convert.ToBoolean((string)data[JKeys.ComfortZone.ValveOpened]);
                        }
                        czn.AttachXMLNode(nsusys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.ComfortZones));
                        czn.OnFloorTemperatureChanged += OnFloorTemperatureChangedHandler;
                        czn.OnRoomTemperatureChanged += OnRoomTemperatureChangedHandler;
                        czn.OnValveOpenedChanged += OnValveOpenedChangedHandler;
                        ComfZones.Add(czn);
                    }
                    break;
                case JKeys.Action.Info:
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
                            case JKeys.ComfortZone.ValveOpened:
                                cz.ValveOpened = (bool)data[JKeys.Generic.Value];
                                break;
                        }
                    }
                    break;
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

        private void OnValveOpenedChangedHandler(ComfortZone sender, bool value)
        {
            NSULog.Debug(LogTag, "OnValveOpenedChangedHandler(). Creating Jobject()");
            JObject jo = new JObject();
            jo[JKeys.Generic.Target] = JKeys.ComfortZone.TargetName;
            jo[JKeys.Generic.Action] = JKeys.Action.Info;
            jo[JKeys.Generic.Name] = sender.Name;
            jo[JKeys.Generic.Content] = JKeys.ComfortZone.ValveOpened;
            jo[JKeys.Generic.Value] = value;
            SendToClient(NetClientRequirements.CreateStandartAcceptInfo(), jo);
        }

        private void OnRoomTemperatureChangedHandler(ComfortZone sender, float value)
        {
            NSULog.Debug(LogTag, "OnRoomTemperatureChangedHandler(). Creating Jobject()");
            JObject jo = new JObject();
            jo[JKeys.Generic.Target] = JKeys.ComfortZone.TargetName;
            jo[JKeys.Generic.Action] = JKeys.Action.Info;
            jo[JKeys.Generic.Name] = sender.Name;
            jo[JKeys.Generic.Content] = JKeys.ComfortZone.CurrentRoomTemp;
            jo[JKeys.Generic.Value] = value;
            SendToClient(NetClientRequirements.CreateStandartAcceptInfo(), jo);
        }

        private void OnFloorTemperatureChangedHandler(ComfortZone sender, float value)
        {
            NSULog.Debug(LogTag, "OnFloorTemperatureChangedHandler(). Creating Jobject()");
            JObject jo = new JObject();
            jo[JKeys.Generic.Target] = JKeys.ComfortZone.TargetName;
            jo[JKeys.Generic.Action] = JKeys.Action.Info;
            jo[JKeys.Generic.Name] = sender.Name;
            jo[JKeys.Generic.Content] = JKeys.ComfortZone.CurrentFloorTemp;
            jo[JKeys.Generic.Value] = value;
            SendToClient(NetClientRequirements.CreateStandartAcceptInfo(), jo);
        }

        public override void Clear()
        {
            ComfZones.Clear();
        }
    }
}
