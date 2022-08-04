using System;
using System.Collections.Generic;
using NSU.Shared.NSUSystemPart;
using NSU.Shared.NSUTypes;
using MySql.Data.MySqlClient;
using NSU.Shared;
using Newtonsoft.Json.Linq;
using NSUWatcher.NSUWatcherNet;
using System.Linq;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class KTypes : NSUSysPartsBase
    {
        readonly string LogTag = "KTypes";
        readonly List<KType> _ktypes = new List<KType>();

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
            return _ktypes.FirstOrDefault(x => x.Name == name);
        }

        private int InsertNewTSensorName(string name)
        {
            int db_id = -1;
            using (MySqlCommand mycmd = nsusys.GetMySqlCmd())
            {
                mycmd.CommandText = "INSERT INTO `tsensor_names`(`type`, `name`) VALUES('ktype', @name)";
                mycmd.Parameters.AddWithValue("@name", name);
                mycmd.ExecuteNonQuery();
                db_id = (int)mycmd.LastInsertedId;
            }
            return db_id;
        }

        private void InsertTemperature(int sid, float value)
        {
            if (sid != -1)
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
        }

        private int GetMySqlIdByName(string name)
        {
            int result = -1;
            if (!name.Equals(string.Empty) && !name.Equals("N"))
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
                    foreach (var item in _ktypes)
                        if (item.DBId != -1) InsertTemperature(item.DBId, (float)item.Temperature);
                    break;

                case JKeys.Action.Info:
                    ProcessActionInfo(data);
                    break;

                case JKeys.Action.Snapshot:
                    ProcessActionSnapshot(data);
                    break;
            }
        }

        private void ProcessActionInfo(JObject data)
        {
            var ktp = FindKType((string)data[JKeys.Generic.Name]);
            if (ktp != null)
            {
                var t = Convert.ToInt32((float)data[JKeys.Generic.Value]);
                ktp.Temperature = t;
            }
        }

        private void ProcessActionSnapshot(JObject data)
        {
            if (data.Property(JKeys.Generic.Result) == null)
            {
                var ktp = new KType
                {
                    ConfigPos = JSonValueOrDefault(data, JKeys.Generic.ConfigPos, KType.INVALID_VALUE),
                    Enabled = JSonValueOrDefault(data, JKeys.Generic.Enabled, false),
                    Name = JSonValueOrDefault(data, JKeys.Generic.Name, string.Empty),
                    Interval = JSonValueOrDefault(data, JKeys.KType.Interval, 0)
                };
                if (JSonValueOrDefault(data, JKeys.Generic.Content, JKeys.Content.Config) == JKeys.Content.ConfigPlus)
                {
                    ktp.Temperature = Convert.ToInt32((float)data[JKeys.Generic.Value]);
                }
                ktp.AttachXMLNode(nsusys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.KTypes));
                //Add sensor events
                ktp.TempChanged += Ktp_TempChanged;
                _ktypes.Add(ktp);
            }
            else
            {
                if ((string)data[JKeys.Generic.Result] == JKeys.Result.Done)
                {
                    foreach (var item in _ktypes)
                        item.DBId = GetMySqlIdByName(item.Name);
                }
            }
        }

        private void Ktp_TempChanged(object sender, TempChangedEventArgs e)
        {
            var vs = new JObject
            {
                { JKeys.Generic.Target, JKeys.KType.TargetName },
                { JKeys.Generic.Action, JKeys.Action.Info },
                { JKeys.Generic.Name, (sender as KType).Name },
                { JKeys.Generic.Value, e.Temperature }
            };
            SendToClient(NetClientRequirements.CreateStandartAcceptInfo(), vs);
        }

        public override void Clear()
        {
            _ktypes.Clear();
        }
    }
}
