using System;
using System.Collections.Generic;
using NSU.Shared.NSUSystemPart;
using NSU.Shared.NSUTypes;
using NSU.Shared;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using NSUWatcher.NSUWatcherNet;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class CircPumps : NSUSysPartsBase
	{
		readonly string LogTag = "CircPumps";

        List<CircPump> circpumps = new List<CircPump>();

		public CircPumps(NSUSys sys, PartsTypes type)
            : base(sys, type)
        {
        }

		public override string[] RegisterTargets()
		{
			return new string[] { JKeys.CircPump.TargetName, "CIRCPUMP:" };

		}

		public CircPump FindCircPump(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            for (int i = 0; i < circpumps.Count; i++)
            {
                CircPump cpc = circpumps[i];
                if(cpc.Name.Equals(name))
                {
                    return cpc;
                }
            }
            return null;
        }

		public CircPump FindCircPump(int cfg_pos)
		{
            if (cfg_pos == CircPump.INVALID_VALUE) return null;
			for (int i = 0; i < circpumps.Count; i++)
			{
				CircPump cpc = circpumps[i];
				if (cpc.ConfigPos == cfg_pos)
				{
					return cpc;
				}
			}
			return null;
		}

        public int GetDBID(string name, bool insertIfNotExists = true)
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
                            cmd.CommandText = "SELECT id FROM `status_names` WHERE `type`='circpump' AND `name`=@cpname";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@cpname", name);
                            var dbid = cmd.ExecuteScalar();
                            if (dbid == null && insertIfNotExists)
                            {
                                cmd.CommandText = "INSERT INTO `status_names`(`type`, `name`) VALUES('circpump', @cpname)";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@cpname", name);
                                cmd.ExecuteNonQuery();
                                result = (int)cmd.LastInsertedId;
                            }
                            result = (int)dbid;
                        }
                    }
                    catch (Exception e)
                    {
                        NSULog.Exception(LogTag, "GetDBID((name)'" + name + "') - " + e.ToString());
                    }
                }
            }
            return result;
        }

        private int UpdateOrInsertName(int dbid, string new_name)
        {
            int result = dbid;
            lock (nsusys.MySQLLock)
            {
                if (dbid >= 0)//update
                {
                    try
                    {
                        using (MySqlCommand cmd = nsusys.GetMySqlCmd())
                        {
                            cmd.CommandText = "UPDATE `status_name` SET `name` = @cpname WHERE id=@cpid;";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@cpname", new_name);
                            cmd.Parameters.AddWithValue("@cpid", dbid);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch(Exception ex)
                    {
                        NSULog.Exception(LogTag, "UPDATE: " + ex.Message);
                    }
                }
                else//insert
                {
                    try
                    {
                        using (MySqlCommand cmd = nsusys.GetMySqlCmd())
                        {
                            cmd.CommandText = "INSERT INTO `status_names`(`type`, `name`) VALUES('circpump', @cpname);";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@cpname", new_name);
                            cmd.ExecuteNonQuery();
                            result = (int)cmd.LastInsertedId;
                        }
                    }
                    catch(Exception ex)
                    {
                        NSULog.Exception(LogTag, "INSERT: " + ex.Message);
                    }
                }
            }
            return result;
        }

        private void InsertStatusValue(int dbid, Status status)
        {
            lock (nsusys.MySQLLock)
            {
                try
                {
                    if (dbid != -1)
                    {
                        using (MySqlCommand cmd = nsusys.GetMySqlCmd())
                        {
                            cmd.CommandText = string.Format("INSERT INTO `status`(`sid`, `value`) VALUE({0}, {1});", dbid, (int)status);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception e)
                {
                    NSULog.Exception(LogTag, "InsertStatusValue() - " + e.ToString());
                }
            }
        }

        public override void ProccessNetworkData(NetClientData clientData, JObject data)
        {
            try
            {
                CircPump cp;
                switch ((string)data[JKeys.Generic.Action])
                {
                    case JKeys.Action.Click:
                        cp = FindCircPump((string)data[JKeys.Generic.Name]);
                        if(cp != null)
                        {
                            NetClientRequirements req = NetClientRequirements.CreateStandart();
                            //TODO Check user rights
                            if (NetClientRequirements.Check(req, clientData))
                            {
                                cp.Clicked();
                                return;
                            }
                            else
                            {
                                var vs = new JObject();
                                if (data.Property(JKeys.Generic.CommandName) != null)
                                    vs.Add(JKeys.Generic.CommandName, (string)data[JKeys.Generic.CommandName]);
                                if (data.Property(JKeys.Generic.CommandID) != null)
                                    vs.Add(JKeys.Generic.CommandID, (string)data[JKeys.Generic.CommandID]);
                                vs.Add(JKeys.Generic.Target, JKeys.CircPump.TargetName);
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
                jo.Add(JKeys.Generic.Target, JKeys.CircPump.TargetName);
                jo = nsusys.SetLastCmdID(clientData, jo);
                jo.Add(JKeys.Generic.Result, false);
                jo.Add(JKeys.Generic.ErrCode, "exception");
                jo.Add(JKeys.Generic.Message, ex.Message);
                nsusys.SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo, string.Empty);
            }
        }

        private void CP_OnClicked(object sender, EventArgs e)
        {
            NSULog.Debug(LogTag, $"CP_OnClicked(). Name: {(sender as CircPump).Name}. Sending to Arduino.");
            var vs = new JObject();
            vs.Add(JKeys.Generic.Target, JKeys.CircPump.TargetName);
            vs.Add(JKeys.Generic.Action, JKeys.CircPump.ActionClick);
            vs.Add(JKeys.Generic.Name, (sender as CircPump).Name);
            SendToArduino(vs);
        }

        private void CP_OnStatusChanged(object sender, StatusChangedEventArgs e)
        {
            InsertStatusValue((sender as CircPump).DbId, e.Status);
            var vs = new JObject();
            vs.Add(JKeys.Generic.Target, JKeys.CircPump.TargetName);
            vs.Add(JKeys.Generic.Action, JKeys.Action.Info);
            vs.Add(JKeys.Generic.Name, (sender as CircPump).Name);
            vs.Add(JKeys.Generic.Status, (sender as CircPump).Status.ToString());
            vs.Add(JKeys.CircPump.CurrentSpeed, (sender as CircPump).CurrentSpeed.ToString());
            vs.Add(JKeys.CircPump.ValvesOpened, (sender as CircPump).OpenedValvesCount.ToString());
            SendToClient(NetClientRequirements.CreateStandartAcceptInfo(), vs);
        }

        public override void ProccessArduinoData(JObject data)
        {
            try
            {
                CircPump cp;
                switch ((string)data[JKeys.Generic.Action])
                {
                    case JKeys.Syscmd.Snapshot:
                        if (data.Property(JKeys.Generic.Result) == null)
                        {
                            cp = new CircPump();
                            cp.ConfigPos = JSonValueOrDefault(data, JKeys.Generic.ConfigPos, NSUPartBase.INVALID_VALUE);
                            cp.Enabled = Convert.ToBoolean(JSonValueOrDefault(data, JKeys.Generic.Enabled, (byte)0));
                            cp.Name = JSonValueOrDefault(data, JKeys.Generic.Name, string.Empty);
                            cp.MaxSpeed = JSonValueOrDefault(data, JKeys.CircPump.MaxSpeed, NSUPartBase.INVALID_VALUE);
                            cp.Spd1Channel = JSonValueOrDefault(data, JKeys.CircPump.Speed1Ch, NSUPartBase.INVALID_VALUE);
                            cp.Spd2Channel = JSonValueOrDefault(data, JKeys.CircPump.Speed2Ch, CircPump.INVALID_VALUE);
                            cp.Spd3Channel = JSonValueOrDefault(data, JKeys.CircPump.Speed3Ch, CircPump.INVALID_VALUE);
                            cp.TempTriggerName = JSonValueOrDefault(data, JKeys.CircPump.TempTriggerName, string.Empty);
                            
                            if (JSonValueOrDefault(data, JKeys.Generic.Content, JKeys.Content.Config) == JKeys.Content.ConfigPlus)
                            {
                                cp.Status = NSU.Shared.NSUUtils.Utils.GetStatusFromString(JSonValueOrDefault(data, JKeys.Generic.Status, string.Empty), Status.UNKNOWN);
                                cp.CurrentSpeed = JSonValueOrDefault(data, JKeys.CircPump.CurrentSpeed, (byte)0);
                                cp.OpenedValvesCount = JSonValueOrDefault(data, JKeys.CircPump.ValvesOpened, (byte)0);
                            }
                            cp.AttachXMLNode(nsusys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.CirculationPumps));
                            circpumps.Add(cp);
                            cp.OnStatusChanged += CP_OnStatusChanged;
                            cp.OnClicked += CP_OnClicked;
                        }
                        else
                        {
                            if (((string)data[JKeys.Generic.Result]).Equals(JKeys.Result.Done))
                            {
                                foreach (var item in circpumps)
                                {
                                    item.DbId = GetDBID(item.Name);
                                    InsertStatusValue(item.DbId, item.Status);
                                }
                            }
                        }
                        return;
                    case JKeys.Action.Info:
                        cp = FindCircPump((string)data[JKeys.Generic.Name]);
                        if(cp != null)
                        {
                            cp.CurrentSpeed = JSonValueOrDefault(data, JKeys.CircPump.CurrentSpeed, (byte)0);
                            cp.OpenedValvesCount = JSonValueOrDefault(data, JKeys.CircPump.ValvesOpened, (byte)0);
                            cp.Status = NSU.Shared.NSUUtils.Utils.GetStatusFromString(JSonValueOrDefault(data, JKeys.Generic.Status, string.Empty), Status.UNKNOWN);
                        }
                        return;
                }
            }
            catch (Exception e)
            {
                NSULog.Exception(LogTag, "ParseArduinoData() - " + e.Message);
            }
        }

        public override void Clear()
        {
            circpumps.Clear();
        }
    }
}

