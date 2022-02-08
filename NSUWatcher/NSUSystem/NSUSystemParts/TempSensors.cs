using System;
using System.Collections.Generic;
using NSU.Shared.NSUTypes;
using NSU.Shared.NSUSystemPart;
using NSUWatcher.NSUWatcherNet;
using System.Collections;
using NSU.Shared;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{

    public class TempSensors : NSUSysPartsBase
    {
        readonly string LogTag = "TempSensors";

        List<TempSensor> Sensors = new List<TempSensor>();
        BitArray ConfigIDs = new BitArray(PartsConsts.MAX_TEMP_SENSORS);
        TempSensor sensor;

        public TempSensors(NSUSys sys, PartsTypes type) : base(sys, type)
        {
        }

        public override string[] RegisterTargets()
        {
            return new string[] { "tsensor" };
        }

        public void AddSensor(TempSensor value)
        {
            if (Sensors.Count < PartsConsts.MAX_TEMP_SENSORS)
            {
                Sensors.Add(value);
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        public TempSensor FindSensor(string name)
        {
            for (int i = 0; i < Sensors.Count; i++)
            {
                sensor = (TempSensor)Sensors[i];
                if (sensor != null && sensor.Name.Equals(name))
                {
                    return sensor;
                }
            }
            return null;
        }

        public TempSensor FindSensor(byte[] saddr)
        {
            for (int i = 0; i < Sensors.Count; i++)
            {
                sensor = (TempSensor)Sensors[i];
                if (sensor != null && sensor.CompareAddr(saddr))
                {
                    return sensor;
                }
            }
            return null;
        }

        public TempSensor FindSensor(byte config_pos)
        {
            for (int i = 0; i < Sensors.Count; i++)
            {
                sensor = (TempSensor)Sensors[i];
                if (sensor != null && sensor.ConfigPos == config_pos)
                {
                    return sensor;
                }
            }
            return null;
        }

        //public void UpdateSensorInfo (byte[] saddr, string new_name, int new_interval)
        public TempSensor UpdateSensorInfo(TempSensor new_info)
        {
            if (new_info == null)
            {
                NSULog.Error(LogTag, "UpdateSensorInfo() - New TSensorInfo is null.");
                return null;
            }

            sensor = FindSensor(new_info.SensorID);
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

        private void SetTemperatureDB(TempSensor ts, float value)
        {
            if (ts != null)
            {
                if (ts.DBId != -1)
                {
                    TimeSpan tspan = DateTime.Now.Subtract(ts.LastDBLog);
                    if (tspan.Minutes > 2)
                    {
                        lock (nsusys.MySQLLock)
                        {
                            using (MySqlCommand cmd = nsusys.GetMySqlCmd())
                            {
                                cmd.Parameters.Clear();
                                cmd.CommandText = "INSERT INTO `temperatures` (`sid`, `value`) VALUES (@sid, @temp)";
                                cmd.Parameters.AddWithValue("@sid", ts.DBId);
                                cmd.Parameters.AddWithValue("@temp", value);
                                try
                                {
                                    cmd.ExecuteNonQuery();
                                    //NSULog.Debug(LogTag, "TSensor successfuly updated to DB.");
                                }
                                catch (Exception e)
                                {
                                    NSULog.Exception(LogTag, "SetTemperature() MySQL error: " + e);
                                }
                                ts.UpdateDBTime();
                            }
                        }
                    }
                }
                else
                {
                    NSULog.Error(LogTag, "TSensors.SetTemperature() - Sensor DBId is -1");
                }
            }
            else
            {
                NSULog.Error(LogTag, "SetTemperature() - TSensorInfo is null.");
            }
        }


        bool IsAddrValid(byte[] addr)
        {
            for (int i = 0; i < 8; i++)
            {
                if (addr[i] != 0)
                {
                    return true;
                }
            }
            return false;
        }

        int InsertNewTSensorName(string name)
        {
            int db_id = -1;
            try
            {
                using (MySqlCommand mycmd = nsusys.GetMySqlCmd())
                {
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
                lock (nsusys.MySQLLock)
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
            }
            return result;
        }

        private int GetFirstFreeConfigID()
        {
            for (int i = 0; i < ConfigIDs.Count; i++)
            {
                if (!ConfigIDs.Get(i))
                    return i;
            }
            return -1;
        }

        private void UpdateConfigIDs()
        {
            for (int i = 0; i < Sensors.Count; i++)
            {
                sensor = (TempSensor)Sensors[i];
                if (sensor.ConfigPos != -1)
                {
                    ConfigIDs.Set(sensor.ConfigPos, true);
                }
            }
            for (int i = 0; i < Sensors.Count; i++)
            {
                sensor = (TempSensor)Sensors[i];
                if (sensor.ConfigPos == -1)
                {
                    int free_id = GetFirstFreeConfigID();
                    if (free_id != -1)
                    {
                        NSULog.Error(LogTag, "Sensor with invalid config_pos found - " + TempSensor.AddrToString(sensor.SensorID));
                        sensor.ConfigPos = free_id;
                        ConfigIDs.Set(free_id, true);
                    }
                }
            }
        }

        public override void ProccessNetworkData(NetClientData clientData, JObject data)
        {
            if (data.Property(JKeys.Generic.Action) == null)
            {

            }
            var vs = new JObject();
            vs.Add(JKeys.Generic.CommandID, (string)data[JKeys.Generic.CommandID]);
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
                    foreach (var item in Sensors)
                    {
                        if (item.DBId != -1)
                        {
                            SetTemperatureDB(item, item.Temperature);
                        }
                    }
                    break;
                case JKeys.Action.Info:
                    var ts = FindSensor(TempSensor.StringToAddr((string)data[JKeys.TempSensor.SensorID]));
                    if (ts != null)
                    {
                        ts.Temperature = (float)data[JKeys.Generic.Value];
                    }
                    break;
                case JKeys.Action.Snapshot:
                    if (data.Property(JKeys.Generic.Result) != null &&
                        ((string)data[JKeys.Generic.Result]).Equals(JKeys.Result.Done))
                    {
                        foreach (var item in Sensors)
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
                                ts = new TempSensor();
                                ts.SensorID = TempSensor.StringToAddr((string)data[JKeys.TempSensor.SensorID]);
                                ts.Temperature = (float)data[JKeys.TempSensor.Temperature];
                                ts.AttachXMLNode(nsusys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.TSensors));
                                Sensors.Add(ts);
                                break;
                            case JKeys.TempSensor.ContentConfig:
                                ts = FindSensor(TempSensor.StringToAddr((string)data[JKeys.TempSensor.SensorID]));
                                if (ts != null)
                                {
                                    ts.ConfigPos = (int)data[JKeys.Generic.ConfigPos];
                                    ts.Name = (string)data[JKeys.Generic.Name];
                                    ts.Interval = (int)data[JKeys.TempSensor.Interval];
                                    ts.NotFound = false;
                                    //ts.AttachXMLNode(nsusys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.TSensors));
                                    //Add sensor events
                                    ts.OnEnabledChanged += TS_EnabledChanged;
                                    ts.OnIntervalChanged += TS_IntervalChanged;
                                    ts.OnNameChanged += TS_NameChanged;
                                    ts.OnTempChanged += TS_TempChanged;
                                }
                                break;
                        }
                    }
                    break;
            }
        }

        private void TS_TempChanged(TempSensor sender, float temp)
        {
            //SetTemperatureDB(sender, temp);
            var vs = new JObject();
            vs.Add(JKeys.Generic.Target, JKeys.TempSensor.TargetName);
            vs.Add(JKeys.Generic.Action, JKeys.Action.Info);
            vs.Add(JKeys.TempSensor.SensorID, TempSensor.AddrToString(sender.SensorID));
            vs.Add(JKeys.Generic.Value, temp);
            SendToClient(NetClientRequirements.CreateStandartAcceptInfo(), vs);
        }

        private void TS_NameChanged(TempSensor sender, string oldname, string newname)
        {

        }

        private void TS_IntervalChanged(TempSensor sender, int interval)
        {

        }

        private void TS_EnabledChanged(TempSensor sender, bool enabled)
        {

        }

        public override void Clear()
        {
            Sensors.Clear();
        }
    }
}

