using System;
using System.Collections.Generic;
using NSU.Shared.NSUTypes;
using NSU.Shared.NSUSystemPart;
using NSUWatcher.NSUWatcherNet;
using System.Collections;
using NSU.Shared;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{

    public class TempSensors : NSUSysPartsBase
    {
        readonly string LogTag = "TempSensors";

        private readonly List<TempSensor> _sensors = new List<TempSensor>();
        readonly BitArray _configIDs = new BitArray(PartsConsts.MAX_TEMP_SENSORS);

        public TempSensors(NSUSys sys, PartsTypes type) : base(sys, type)
        {
        }

        public override string[] RegisterTargets()
        {
            return new string[] { "tsensor" };
        }

        public void AddSensor(TempSensor value)
        {
            if (_sensors.Count < PartsConsts.MAX_TEMP_SENSORS)
            {
                _sensors.Add(value);
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        public TempSensor FindSensor(string name)
        {
            return _sensors.Find(x => x.Name == name);
        }

        public TempSensor FindSensor(byte[] addr)
        {
            return _sensors.Find(x => x.CompareAddr(addr));
        }

        public TempSensor FindSensor(byte cfgPos)
        {
            return _sensors.Find(x => x.ConfigPos == cfgPos);
        }

        public TempSensor UpdateSensorInfo(TempSensor new_info)
        {
            if (new_info == null)
            {
                NSULog.Error(LogTag, "UpdateSensorInfo() - New TSensorInfo is null.");
                return null;
            }

            var sensor = FindSensor(new_info.SensorID);
            if (sensor != null)
            {
                if (!new_info.Name.Equals(sensor.Name))
                {
                    int db_id = sensor.DBId;
                    try
                    {
                        if (db_id != -1)
                        {
                            using (MySqlCommand mycmd = nsusys.GetMySqlCmd())
                            {
                                if (new_info.Name.Equals(string.Empty))
                                {
                                    mycmd.CommandText = string.Format("DELETE FROM `tsensor_names` WHERE id={0}", db_id);
                                    db_id = -1;
                                }
                                else
                                {
                                    mycmd.CommandText = string.Format("UPDATE `tsensor_names` SET `name`='{0}' WHERE id={1}", new_info.Name, db_id);
                                }
                                mycmd.ExecuteNonQuery();
                            }
                        }
                        else
                        if (!new_info.Name.Equals(string.Empty))
                        {
                            db_id = GetMySqlIDByName(new_info.Name);
                            if (db_id == -1)
                            {
                                using (MySqlCommand mycmd = nsusys.GetMySqlCmd())
                                {
                                    mycmd.CommandText = string.Format("INSERT INTO `tsensor_names`(`name`) VALUES('{0}')", new_info.Name);
                                    mycmd.ExecuteNonQuery();
                                    db_id = (int)mycmd.LastInsertedId;
                                }
                            }
                        }
                        else
                        {
                            NSULog.Error(LogTag, "UpdateSensorInfo('" + new_info + "') - New sensor must have name.");
                            return null;
                        }
                        sensor.DBId = db_id;
                    }
                    catch (Exception e)
                    {
                        NSULog.Exception(LogTag, "UpdateSensorInfo('" + new_info + "') - " + e);
                    }
                }
                sensor.Enabled = new_info.Enabled;
                sensor.Name = new_info.Name;
                sensor.Interval = new_info.Interval;
            }
            else
            {
                NSULog.Error(LogTag, "TSensors.UpdateSensorInfo('" + new_info + "') - invalid address. ");
            }
            return sensor;
        }

        private void InsertTemperatureToDB(TempSensor ts, float value)
        {
            TimeSpan tspan = DateTime.Now.Subtract(ts.LastDBLog);
            if (tspan.Minutes > 2)
            {
                using (MySqlCommand cmd = nsusys.GetMySqlCmd())
                {
                    if (cmd == null) return;
                    cmd.Parameters.Clear();
                    cmd.CommandText = "INSERT INTO `temperatures` (`sid`, `value`) VALUES (@sid, @temp)";
                    cmd.Parameters.AddWithValue("@sid", ts.DBId);
                    cmd.Parameters.AddWithValue("@temp", value);
                    cmd.ExecuteNonQuery();
                    ts.UpdateDBTime();
                }
            }
        }


        bool IsAddrValid(byte[] addr)
        {
            return !addr.All(x => x == 0);
        }

        int InsertNewTSensorName(string name)
        {
            int db_id = -1;
            try
            {
                using (MySqlCommand mycmd = nsusys.GetMySqlCmd())
                {
                    if (mycmd == null) return db_id;
                    mycmd.CommandText = string.Format("INSERT INTO `tsensor_names`(`type`, `name`) VALUES('ds18b20', '{0}')", name);
                    mycmd.ExecuteNonQuery();
                    db_id = (int)mycmd.LastInsertedId;
                }
            }
            catch (Exception e)
            {
                NSULog.Exception(LogTag, "InsertNewTSensorName('" + name + "') - " + e);
            }
            return db_id;
        }

        int GetMySqlIDByName(string name)
        {
            int result = -1;
            if (!name.Equals(string.Empty) && !name.Equals("N"))
            {
                using (MySqlCommand cmd = nsusys.GetMySqlCmd())
                {
                    cmd.CommandText = string.Format("SELECT id FROM `tsensor_names` WHERE `type`='ds18b20' AND `name`='{0}';", name);
                    var dbid = cmd.ExecuteScalar();
                    if (dbid == null)
                    {
                        result = InsertNewTSensorName(name);
                    }
                    else
                    {
                        result = (int)dbid;
                    }
                }
            }
            return result;
        }

        private int GetFirstFreeConfigID()
        {
            for (int i = 0; i < _configIDs.Count; i++)
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
                int freePos = GetFirstFreeConfigID();
                if (freePos != NSUPartBase.INVALID_VALUE)
                {
                    sensor.ConfigPos = freePos;
                    _configIDs[freePos] = true;
                    NSULog.Error(LogTag, $"Sensor '{TempSensor.AddrToString(sensor.SensorID)}' with invalid ConfigPos founded. New ConfigPos is {freePos}.");
                }
                else
                {
                    NSULog.Error(LogTag, $"No more free indexes for config is left. Cannot assign ConfigPos to a sensor '{TempSensor.AddrToString(sensor.SensorID)}'");
                }
            }
        }

        public override void ProccessNetworkData(NetClientData clientData, JObject data)
        {
            //TODO Configure sensor from "outside"
            if (data.Property(JKeys.Generic.Action) == null)
            {
                
            }
            var jo = new JObject();
            jo.Add(JKeys.Generic.CommandID, (string)data[JKeys.Generic.CommandID]);
            switch ((string)data[JKeys.Generic.Action])
            {
                case JKeys.Action.Get:
                case JKeys.Action.Set:
                case JKeys.Action.Clear:
                    break;
            }
        }

        public override void ProccessArduinoData(JObject data)
        {
            switch ((string)data[JKeys.Generic.Action])
            {
                case JKeys.Timer.ActionTimer:
                    foreach (var item in _sensors.Where(item => item.DBId != -1))
                    {
                        InsertTemperatureToDB(item, item.Temperature);
                    }

                    break;
                case JKeys.Action.Info:
                    ProcessArdSetInfo(data);
                    break;

                case JKeys.Action.Snapshot:
                    if (data.Property(JKeys.Generic.Result) != null &&
                        ((string)data[JKeys.Generic.Result]) == JKeys.Result.Done)
                    {
                        foreach (var item in _sensors)
                        {
                            item.DBId = GetMySqlIDByName(item.Name);
                        }
                        UpdateConfigIDs();
                        return;
                    }
                    else
                    {
                        switch ((string)data[JKeys.Generic.Content])
                        {
                            case JKeys.TempSensor.ContentSystem:
                                ProcessArdSnapshotSystemScan(data);
                                break;

                            case JKeys.TempSensor.ContentConfig:
                                ProcessArdSnapshotConfig(data);

                                break;
                        }
                    }
                    break;
            }
        }

        private void ProcessArdSetInfo(JObject data)
        {
            var ts = FindSensor(TempSensor.StringToAddr((string)data[JKeys.TempSensor.SensorID]));
            if (ts != null)
            {
                ts.ReadErrorCount = (int)(data[JKeys.TempSensor.ReadErrors] ?? 0);
                ts.Temperature = (float)(data[JKeys.Generic.Value] ?? 0);
            }
        }

        private void ProcessArdSnapshotSystemScan(JObject data)
        {
            var ts = new TempSensor();
            ts.SensorID = TempSensor.StringToAddr(JSonValueOrDefault(data, JKeys.TempSensor.SensorID, TempSensor.NullAddress));
            ts.Temperature = JSonValueOrDefault(data, JKeys.TempSensor.Temperature, 0.0f);
            ts.ReadErrorCount = JSonValueOrDefault(data, JKeys.TempSensor.ReadErrors, 0);
            ts.AttachXMLNode(nsusys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.TSensors));
            _sensors.Add(ts);
        }

        private void ProcessArdSnapshotConfig(JObject data)
        {
            var ts = FindSensor(TempSensor.StringToAddr((string)data[JKeys.TempSensor.SensorID]));//NullAddress sensors is filtered here
            if (ts == null)
            {
                ts = new TempSensor();
                ts.AttachXMLNode(nsusys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.TSensors));
                if (!TempSensor.IsAddressNull(ts))
                    ts.NotFound = true;
                _sensors.Add(ts);
            }
            else
            {
                ts.NotFound = false;
            }
            ts.ConfigPos = JSonValueOrDefault(data, JKeys.Generic.ConfigPos, NSUPartBase.INVALID_VALUE);
            ts.Enabled = Convert.ToBoolean(JSonValueOrDefault(data, JKeys.Generic.Enabled, (byte)0));
            ts.SensorID = TempSensor.StringToAddr(JSonValueOrDefault(data, JKeys.TempSensor.SensorID, TempSensor.NullAddress));
            ts.Name = JSonValueOrDefault(data, JKeys.Generic.Name, string.Empty);
            ts.Interval = JSonValueOrDefault(data, JKeys.TempSensor.Interval, 0);

            //Add sensor events
            ts.OnEnabledChanged += TS_EnabledChanged;
            ts.OnIntervalChanged += TS_IntervalChanged;
            ts.OnNameChanged += TS_NameChanged;
            ts.OnTempChanged += TS_TempChanged;
        }

        private void TS_TempChanged(object sender, TempChangedEventArgs e)
        {
            var jo = new JObject
            {
                [JKeys.Generic.Target] = JKeys.TempSensor.TargetName,
                [JKeys.Generic.Action] = JKeys.Action.Info,
                [JKeys.TempSensor.SensorID] = TempSensor.AddrToString((sender as TempSensor).SensorID),
                [JKeys.Generic.Value] = e.Temperature
            };
            SendToClient(NetClientRequirements.CreateStandartAcceptInfo(), jo);
        }

        private void TS_NameChanged(object sender, NameChangedEventArgs e)
        {

        }

        private void TS_IntervalChanged(object sender, IntervalChangedEventArgs e)
        {

        }

        private void TS_EnabledChanged(object sender, EnabledChangedEventArgs e)
        {

        }

        public override void Clear()
        {
            _sensors.Clear();
        }
    }
}

