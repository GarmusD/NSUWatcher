using System;
using System.Collections.Generic;
using NSU.Shared.NSUSystemPart;
using NSU.Shared.NSUTypes;
using NSUWatcher.NSUWatcherNet;
using MySql.Data.MySqlClient;
using NSU.Shared;
using Newtonsoft.Json.Linq;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class Switches : NSUSysPartsBase
    {
        readonly string LogTag = "Switches";
        List<Switch> switches = new List<Switch>();
        Switch sw;
		public Switches(NSUSys sys, PartsTypes type)
            : base(sys, type)
        {
        }

        public override string[] RegisterTargets()
        {
            return new string[] { "switch", "SWITCH:" };
        }


        private Switch FindSwitch(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            for (int i = 0; i < switches.Count; i++)
            {
                var swth = (Switch)switches[i];
                if (swth.Name.Equals(name))
                    return swth;
            }
            return null;
        }

        private Switch FindSwitch(byte cfg_pos)
        {
            if (cfg_pos == Switch.INVALID_VALUE) return null;
            for (int i = 0; i < switches.Count; i++)
            {
                var swth = (Switch)switches[i];
                if (swth.ConfigPos == cfg_pos)
                    return swth;
            }
            return null;
        }

        private int GetDBID(string name, bool insertIfNotExists = true)
        {
            int result = -1;
            if (!name.Equals(string.Empty) && !name.Equals("N"))
            {
                lock (nsusys.MySQLLock)
                {
                    try
                    {
                        using (MySqlCommand cmd = nsusys.GetMySqlCmd())
                        {
                            cmd.CommandText = string.Format("SELECT id FROM `status_names` WHERE `type`='switch' AND `name`='{0}';", name);
                            var dbid = cmd.ExecuteScalar();
                            if (dbid == null && insertIfNotExists)
                            {
                                cmd.CommandText = string.Format("INSERT INTO `status_names`(`type`, `name`) VALUES('switch', '{0}');", name);
                                cmd.ExecuteNonQuery();
                                result = (int)cmd.LastInsertedId;
                            }
                            else
                            {
                                result = (int)dbid;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        NSULog.Exception(LogTag, "GetDBID('" + name + "') - " + e);
                    }
                }
            }
            return result;
        }

        private void InsertStatusValue(int dbid, int value)
        {
            if (dbid == -1)
                return;
            lock (nsusys.MySQLLock)
            {
                try
                {
                    using (MySqlCommand cmd = nsusys.GetMySqlCmd())
                    {
                        cmd.CommandText = string.Format("INSERT INTO `status` (`sid`, `value`) VALUES({0}, {1});", dbid, value);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    NSULog.Exception(LogTag, string.Format("InsertStatusValue((dbid){0}, (value){1}) - {2}", dbid, value, e));
                }
            }
        }

        public override void ProccessArduinoData(JObject data)
        {
            try
            {
                string act = (string)data[JKeys.Generic.Action];
                switch (act)
                {
                    case JKeys.Syscmd.Snapshot:
                        if (data.Property(JKeys.Generic.Result) == null)
                        {
                            sw = new Switch();
                            sw.ConfigPos = JSonValueOrDefault(data, JKeys.Generic.ConfigPos, NSUPartBase.INVALID_VALUE);
                            sw.Enabled = Convert.ToBoolean(JSonValueOrDefault(data, JKeys.Generic.Enabled, (byte)0));
                            sw.Name = JSonValueOrDefault(data, JKeys.Generic.Name, string.Empty);
                            sw.Dependancy = JSonValueOrDefault(data, JKeys.Switch.Dependancy, string.Empty);
                            sw.OnDependancyStatus = NSU.Shared.NSUUtils.Utils.GetStatusFromString(JSonValueOrDefault(data, JKeys.Switch.OnDependancyStatus, string.Empty), Status.UNKNOWN);
                            sw.ForceStatus = NSU.Shared.NSUUtils.Utils.GetStatusFromString(JSonValueOrDefault(data, JKeys.Switch.ForceStatus, string.Empty), Status.UNKNOWN);

                            if (JSonValueOrDefault(data, JKeys.Generic.Content, JKeys.Content.Config) == JKeys.Content.ConfigPlus)
                            {
                                sw.IsForced = JSonValueOrDefault(data, JKeys.Switch.IsForced, false);
                                sw.Status = NSU.Shared.NSUUtils.Utils.GetStatusFromString(JSonValueOrDefault(data, JKeys.Switch.CurrState, string.Empty), Status.UNKNOWN);
                            }

                            sw.AttachXMLNode(nsusys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.Switches));
                            sw.OnStatusChanged += Switch_OnStatusChanged;
                            sw.OnClicked += Switch_OnClicked;
                            switches.Add(sw);
                        }
                        else
                        {
                            if (((string)data[JKeys.Generic.Result]).Equals(JKeys.Result.Done))
                            {
                                foreach (var item in switches)
                                {
                                    item.DbId = GetDBID(item.Name);
                                    //update status
                                    InsertStatusValue(sw.DbId, (int)sw.Status);
                                }
                            }
                        }
                        break;
                    case JKeys.Action.Info:
                        sw = FindSwitch((string)data[JKeys.Generic.Name]);
                        if (sw != null)
                        {
                            //Set isForced first because of OnStatusChange event, which sends unchecked isForced
                            sw.IsForced = (bool)data[JKeys.Switch.IsForced];
                            sw.Status = NSU.Shared.NSUUtils.Utils.GetStatusFromString((string)data[JKeys.Generic.Status], Status.UNKNOWN);
                        }
                        break;
                }
            }
            catch(Exception e)
            {
                NSULog.Exception(LogTag, e.Message + "\r\n" + e.StackTrace.ToString());
            }
        }

        public override void ProccessNetworkData(NetClientData clientData, JObject data)
        {
            try
            {
                Switch sw;
                switch ((string)data[JKeys.Generic.Action])
                {
                    case JKeys.Action.Click:
                        sw = FindSwitch((string)data[JKeys.Generic.Name]);
                        if (sw != null)
                        {
                            NetClientRequirements req = NetClientRequirements.CreateStandart();
                            //TODO Check user rights
                            if (NetClientRequirements.Check(req, clientData))
                            {
                                sw.Clicked();
                                return;
                            }
                            else
                            {
                                var vs = new JObject();
                                if (data.Property(JKeys.Generic.CommandName) != null)
                                    vs.Add(JKeys.Generic.CommandName, (string)data[JKeys.Generic.CommandName]);
                                if (data.Property(JKeys.Generic.CommandID) != null)
                                    vs.Add(JKeys.Generic.CommandID, (string)data[JKeys.Generic.CommandID]);
                                vs.Add(JKeys.Generic.Target, JKeys.Switch.TargetName);
                                vs.Add(JKeys.Generic.Result, false);
                                vs.Add(JKeys.Generic.ErrCode, "access_denied");
                                vs.Add(JKeys.Generic.Message, "Access denied.");
                                nsusys.SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), vs, string.Empty);
                            }
                        }
                        return;
                }
            }
            catch (Exception ex)
            {
                NSULog.Exception(LogTag, "ParseNetworkData() - " + ex.ToString());
                var jo = new JObject();
                jo.Add(JKeys.Generic.Target, JKeys.Switch.TargetName);
                jo = nsusys.SetLastCmdID(clientData, jo);
                jo.Add(JKeys.Generic.Result, false);
                jo.Add(JKeys.Generic.ErrCode, "exception");
                jo.Add(JKeys.Generic.Message, ex.Message);
                nsusys.SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo, string.Empty);
            }
        }

        public override void Clear()
        {
            switches.Clear();
        }

        private void Switch_OnStatusChanged(object sender, SwitchStatusChangedEventArgs e)
        {
            InsertStatusValue((sender as Switch).DbId, (int)e.Status);
            var jo = new JObject();
            jo[JKeys.Generic.Target] = JKeys.Switch.TargetName;
            jo[JKeys.Generic.Action] = JKeys.Action.Info;
            jo[JKeys.Generic.Name] = (sender as Switch).Name;
            jo[JKeys.Generic.Status] = e.Status.ToString();
            jo[JKeys.Switch.IsForced] = e.IsForced;
            SendToClient(NetClientRequirements.CreateStandartAcceptInfo(), jo);
        }

        private void Switch_OnClicked(object sender, EventArgs e)
        {
            NSULog.Debug(LogTag, $"Switch_OnClicked(). Name: {(sender as Switch).Name}. Sending to Arduino.");
            var vs = new JObject();
            vs.Add(JKeys.Generic.Target, JKeys.Switch.TargetName);
            vs.Add(JKeys.Generic.Action, JKeys.Action.Click);
            vs.Add(JKeys.Generic.Name, (sender as Switch).Name);
            SendToArduino(vs);
        }
    }
}

