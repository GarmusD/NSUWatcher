using System;
using System.Collections.Generic;
using NSU.Shared.NSUSystemPart;
using NSU.Shared.NSUTypes;
using MySql.Data.MySqlClient;
using NSU.Shared;
using Newtonsoft.Json.Linq;
using NSUWatcher.NSUWatcherNet;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class KTypes : NSUSysPartsBase
    {
        readonly string LogTag = "KTypes";

        List<KType> ktypes = new List<KType>();

        public KTypes(NSUSys sys, PartsTypes type)
            : base(sys, type)
        {
        }

        public override string[] RegisterTargets()
        {
            return new string[] { JKeys.KType.TargetName };
        }

        public KType FindKType(string name)
        {
            for (int i = 0; i < ktypes.Count; i++)
            {
                if (ktypes[i].Name.Equals(name))
                    return ktypes[i];
            }
            return null;
        }

        private int InsertNewTSensorName(string name)
        {
            int db_id = -1;
            try
            {
                using (MySqlCommand mycmd = nsusys.GetMySqlCmd())
                {
                    mycmd.CommandText = string.Format("INSERT INTO `tsensor_names`(`type`, `name`) VALUES('ktype', '{0}')", name);
                    mycmd.ExecuteNonQuery();
                    db_id = (int)mycmd.LastInsertedId;
                }
            }
            catch (Exception ex)
            {
                NSULog.Exception(LogTag, string.Format("InsertNewTSensorName('{0}') - {1}", name, ex.Message));
            }
            return db_id;
        }

        private void InsertTemperature(int sid, float value)
        {
            if (sid != -1)
            {
                lock (nsusys.MySQLLock)
                {
                    try
                    {
                        using (MySqlCommand cmd = nsusys.GetMySqlCmd())
                        {
                            cmd.Parameters.Clear();
                            cmd.CommandText = "INSERT INTO `temperatures` (`sid`, `value`) VALUES (@sid, @temp)";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@sid", sid);
                            cmd.Parameters.AddWithValue("@temp", value);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        NSULog.Exception(LogTag, ex.Message /*string.Format("InsertTemperature((sid){0}, (value){1}) - {e}", sid, value)*/);
                    }
                }
            }
        }

        private int GetMySqlIdByName(string name)
        {
            int result = -1;
            if (!name.Equals(string.Empty) && !name.Equals("N"))
            {
                lock (nsusys.MySQLLock)
                {
                    using (MySqlCommand cmd = nsusys.GetMySqlCmd())
                    {
                        cmd.CommandText = "SELECT id FROM `tsensor_names` WHERE `type`='ktype' AND `name`=@name;";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@name", name);
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

        public override void ProccessNetworkData(NetClientData clientData, JObject data)
        {

        }

        public override void ProccessArduinoData(JObject data)
        {
            switch ((string)data[JKeys.Generic.Action])
            {
                case JKeys.Timer.ActionTimer:
                    //NSULog.Debug(LogTag, "TimerAction. ktypes.count " + ktypes.Count.ToString());
                    foreach (var item in ktypes)
                    {
                        //NSULog.Debug(LogTag, "TimerAction. Item name " + item.Name + " DBId " + item.DBId.ToString());
                        if (item.DBId != -1)
                        {
                            //NSULog.Debug(LogTag, "TimerAction. Going to insert temperature");
                            InsertTemperature(item.DBId, (float)item.Temperature);
                        }
                    }
                    break;
                case JKeys.Action.Info:
                    try
                    {
                        var ktp = FindKType((string)data[JKeys.Generic.Name]);
                        if (ktp != null)
                        {
                            var t = Convert.ToInt32((float)data[JKeys.Generic.Value]);
                            ktp.Temperature = t;
                        }
                    }
                    catch (Exception ex)
                    {
                        NSULog.Exception(LogTag, ex.Message);
                        throw;
                    }
                    break;
                case JKeys.Action.Snapshot:
                    if (data.Property(JKeys.Generic.Result) == null)
                    {
                        var ktp = new KType();
                        ktp.ConfigPos = (int)data[JKeys.Generic.ConfigPos];
                        ktp.Name = (string)data[JKeys.Generic.Name];
                        ktp.Interval = (int)data[JKeys.KType.Interval];
                        if (data.Property(JKeys.Generic.Value) != null)
                        {
                            ktp.Temperature = Convert.ToInt32((float)data[JKeys.Generic.Value]);
                        }
                        ktp.AttachXMLNode(nsusys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.KTypes));
                        //Add sensor events
                        ktp.OnTempChanged += Ktp_TempChanged;
                        ktypes.Add(ktp);
                    }
                    else
                    {
                        if (((string)data[JKeys.Generic.Result]).Equals(JKeys.Result.Done))
                        {
                            foreach (var item in ktypes)
                            {
                                item.DBId = GetMySqlIdByName(item.Name);
                            }
                        }
                    }
                    break;
            }
        }

        private void Ktp_TempChanged(KType sender, int temp)
        {
            //InsertTemperature(sender.DBId, (float)temp);
            var vs = new JObject();
            vs.Add(JKeys.Generic.Target, JKeys.KType.TargetName);
            vs.Add(JKeys.Generic.Action, JKeys.Action.Info);
            vs.Add(JKeys.Generic.Name, sender.Name);
            vs.Add(JKeys.Generic.Value, temp);
            SendToClient(NetClientRequirements.CreateStandartAcceptInfo(), vs);
        }

        public override void Clear()
        {
            ktypes.Clear();
        }
    }
}
