using System;
using System.Collections.Generic;
using NSU.Shared.NSUSystemPart;
using NSU.Shared.NSUTypes;
using NSUWatcher.NSUWatcherNet;
using MySql.Data.MySqlClient;
using NSU.Shared;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class Switches : NSUSysPartsBase
    {
        readonly string LogTag = "Switches";

        private readonly List<Switch> _switches = new List<Switch>();

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
            return _switches.FirstOrDefault(x => x.Name == name);
        }

        private Switch FindSwitch(byte cfgPos)
        {
            return _switches.FirstOrDefault(x => x.ConfigPos == cfgPos);
        }

        private int GetDBID(string name, bool insertIfNotExists = true)
        {
            int result = -1;
            if (!name.Equals(string.Empty) && !name.Equals("N"))
            {
                using (MySqlCommand cmd = nsusys.GetMySqlCmd())
                {
                    cmd.CommandText = "SELECT id FROM `status_names` WHERE `type`='switch' AND `name`=@name";
                    cmd.Parameters.AddWithValue("@name", name);
                    var dbid = cmd.ExecuteScalar();
                    if (dbid == null && insertIfNotExists)
                    {
                        cmd.CommandText = "INSERT INTO `status_names`(`type`, `name`) VALUES('switch', @name);";
                        //parameter is the same from select command
                        cmd.Prepare();
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
            if (dbid == -1) return;

            using (MySqlCommand cmd = nsusys.GetMySqlCmd())
            {
                cmd.CommandText = "INSERT INTO `status` (`sid`, `value`) VALUES(@sid, @value);";
                cmd.Parameters.AddWithValue("@sid", dbid);
                cmd.Parameters.AddWithValue("@value", value);
                cmd.ExecuteNonQuery();
            }
        }

        public override void ProccessArduinoData(JObject data)
        {
            try
            {
                Switch sw;
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
                            _switches.Add(sw);
                        }
                        else
                        {
                            if (((string)data[JKeys.Generic.Result]).Equals(JKeys.Result.Done))
                            {
                                foreach (var item in _switches)
                                {
                                    item.DbId = GetDBID(item.Name);
                                    InsertStatusValue(item.DbId, (int)item.Status);
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
            catch (Exception e)
            {
                NSULog.Exception(LogTag, e.Message + "\r\n" + e.StackTrace.ToString());
            }
        }

        public override void ProccessNetworkData(NetClientData clientData, JObject data)
        {
            try
            {
                switch ((string)data[JKeys.Generic.Action])
                {
                    case JKeys.Action.Click:
                        Switch sw = FindSwitch((string)data[JKeys.Generic.Name]);
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
                var jo = new JObject
                {
                    [JKeys.Generic.Target] = JKeys.Switch.TargetName,
                    [JKeys.Generic.Result] = false,
                    [JKeys.Generic.ErrCode] = "exception",
                    [JKeys.Generic.Message] = ex.Message
                };
                jo = nsusys.SetLastCmdID(clientData, jo);
                nsusys.SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo, string.Empty);
            }
        }

        public override void Clear()
        {
            _switches.Clear();
        }

        private void Switch_OnStatusChanged(object sender, SwitchStatusChangedEventArgs e)
        {
            InsertStatusValue((sender as Switch).DbId, (int)e.Status);
            var jo = new JObject
            {
                [JKeys.Generic.Target] = JKeys.Switch.TargetName,
                [JKeys.Generic.Action] = JKeys.Action.Info,
                [JKeys.Generic.Name] = (sender as Switch).Name,
                [JKeys.Generic.Status] = e.Status.ToString(),
                [JKeys.Switch.IsForced] = e.IsForced
            };
            SendToClient(NetClientRequirements.CreateStandartAcceptInfo(), jo);
        }

        private void Switch_OnClicked(object sender, EventArgs e)
        {
            NSULog.Debug(LogTag, $"Switch_OnClicked(). Name: {(sender as Switch).Name}. Sending to Arduino.");
            var vs = new JObject
            {
                { JKeys.Generic.Target, JKeys.Switch.TargetName },
                { JKeys.Generic.Action, JKeys.Action.Click },
                { JKeys.Generic.Name, (sender as Switch).Name }
            };
            SendToArduino(vs);
        }
    }
}

