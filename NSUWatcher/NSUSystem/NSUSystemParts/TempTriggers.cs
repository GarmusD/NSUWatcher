using System;
using NSU.Shared.NSUTypes;
using System.Collections.Generic;
using NSU.Shared.NSUSystemPart;
using NSUWatcher.NSUWatcherNet;
using MySql.Data.MySqlClient;
using NSU.Shared;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class TempTriggers : NSUSysPartsBase
    {
        readonly string LogTag = "TempTriggers";

        readonly List<TempTrigger> _triggers = new List<TempTrigger>();

        public TempTriggers(NSUSys sys, PartsTypes type)
            : base(sys, type)
        {
        }

        public override string[] RegisterTargets()
        {
            return new string[] { JKeys.TempTrigger.TargetName, "TRIGGER:" };
        }

        private TempTrigger FindTrigger(string name)
        {
            return _triggers.FirstOrDefault(x => x.Name == name);
        }

        private TempTrigger FindTrigger(int cfgPos)
        {
            return _triggers.FirstOrDefault(x => x.ConfigPos == cfgPos);
        }

        int GetDBID(string name, bool insertIfNotExists = true)
        {
            int result = -1;
            if (!name.Equals(string.Empty))
            {
                using (var cmd = nsusys.GetMySqlCmd())
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT id FROM `status_names` WHERE `type`='trigger' AND `name`=@name;";
                    cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = name;
                    var dbid = cmd.ExecuteScalar();
                    if (dbid == null && insertIfNotExists)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "INSERT INTO `status_names`(`type`, `name`) VALUES('trigger', @name);";
                        cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = name;
                        cmd.ExecuteNonQuery();
                        result = (int)cmd.LastInsertedId;
                    }
                    else
                    {
                        result = (int)dbid;
                    }
                }
            }
            return result;
        }

        private void InsertStatusValue(int dbid, int value)
        {
            if (dbid != -1)
            {
                using (MySqlCommand cmd = nsusys.GetMySqlCmd())
                {
                    cmd.CommandText = string.Format("INSERT INTO `status` (`sid`, `value`) VALUES({0}, {1});", dbid, value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public override void ProccessArduinoData(JObject data)
        {
            TempTrigger trg;
            string act = (string)data[JKeys.Generic.Action];
            switch (act)
            {
                case JKeys.Syscmd.Snapshot:
                    if (data.Property(JKeys.Generic.Result) == null)
                    {
                        trg = new TempTrigger();
                        trg.ConfigPos = JSonValueOrDefault(data, JKeys.Generic.ConfigPos, TempTrigger.INVALID_VALUE);
                        trg.Name = JSonValueOrDefault(data, JKeys.Generic.Name, string.Empty);
                        trg.Enabled = Convert.ToBoolean(JSonValueOrDefault(data, JKeys.Generic.Enabled, (byte)0));

                        JArray ja = (JArray)data[JKeys.TempTrigger.Pieces];
                        for (int i = 0; i < TempTrigger.MAX_TEMPTRIGGERPIECES; i++)
                        {
                            JObject jo = (JObject)ja[i];
                            trg[i].Enabled = Convert.ToBoolean((byte)jo[JKeys.Generic.Enabled]);
                            trg[i].TSensorName = (string)jo[JKeys.TempTrigger.TriggerSensorName];
                            trg[i].Condition = (TriggerCondition)(byte)jo[JKeys.TempTrigger.TriggerCondition];
                            trg[i].Temperature = (float)jo[JKeys.TempTrigger.TriggerTemperature];
                            trg[i].Histeresis = (float)jo[JKeys.TempTrigger.TriggerHisteresis];
                        }
                        if (JSonValueOrDefault(data, JKeys.Generic.Content, JKeys.Content.Config) == JKeys.Content.ConfigPlus)
                            trg.Status = NSU.Shared.NSUUtils.Utils.GetStatusFromString(JSonValueOrDefault(data, JKeys.Generic.Status, string.Empty), Status.UNKNOWN);

                        trg.AttachXMLNode(nsusys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.TempTriggers));
                        trg.OnStatusChanged += Trg_OnStatusChanged;
                        _triggers.Add(trg);
                    }
                    else
                    {
                        if (((string)data[JKeys.Generic.Result]).Equals(JKeys.Result.Done))
                        {
                            foreach (var item in _triggers)
                            {
                                item.DbID = GetDBID(item.Name);
                            }
                        }
                    }
                    break;
                case JKeys.Action.Info:
                    trg = FindTrigger((string)data[JKeys.Generic.Name]);
                    if (trg != null)
                    {
                        trg.Status = NSU.Shared.NSUUtils.Utils.GetStatusFromString(JSonValueOrDefault(data, JKeys.Generic.Status, string.Empty), Status.UNKNOWN);
                    }
                    break;
            }
        }

        private void Trg_OnStatusChanged(TempTrigger sender)
        {
            InsertStatusValue(sender.DbID, (int)sender.Status);
            var req = NetClientRequirements.CreateStandartAcceptInfo();
            JObject jo = new JObject
            {
                [JKeys.Generic.Target] = JKeys.TempTrigger.TargetName,
                [JKeys.Generic.Action] = JKeys.Action.Info,
                [JKeys.Generic.Name] = sender.Name,
                [JKeys.Generic.Status] = sender.Status.ToString()
            };
            SendToClient(req, jo);
        }

        public override void ProccessNetworkData(NetClientData clientData, JObject data)
        {
            var req = NetClientRequirements.CreateStandartClientOnly(clientData);
            switch ((string)data[JKeys.Generic.Action])
            {
                case JKeys.Action.Get:

                    break;
                case JKeys.Action.Set:
                case JKeys.Action.Clear:
                    //check user access

                    return;
                default:
                    break;
            }
        }

        public override void Clear()
        {
            _triggers.Clear();
        }
    }
}

