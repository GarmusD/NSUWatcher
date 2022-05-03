using System;
using System.Collections.Generic;
using NSU.Shared.NSUSystemPart;
using NSU.Shared.NSUTypes;
using NSU.Shared;
using Newtonsoft.Json.Linq;
using MySql.Data.MySqlClient;
using NSUWatcher.NSUWatcherNet;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{

    public class WoodBoilers : NSUSysPartsBase
    {
        readonly string LogTag = "WoodBoilers";
		readonly List<WoodBoiler> boilers;
        WoodBoiler wb;

		public WoodBoilers(NSUSys sys, PartsTypes type)
            : base(sys, type)
        {
            boilers = new List<WoodBoiler>();
        }

        public override string[] RegisterTargets()
        {
            return new string[] {JKeys.WoodBoiler.TargetName};
        }

		public WoodBoiler FindWoodBoiler(string name)
		{
            NSULog.Debug(LogTag, $"Searching for WoodBoiler '{name}'");
			foreach (var item in boilers)
			{
				if (item.Name.Equals(name))
				{
					return item;
				}
			}
			return null;
		}

		public WoodBoiler FindWoodBoiler(byte cfg_pos)
		{
			foreach (var item in boilers)
			{
				if (item.ConfigPos == cfg_pos)
				{
					return item;
				}
			}
			return null;
		}

        int GetDBID(string name, string type, bool insertIfNotExists = true)
        {
            int result = -1;
            if (!name.Equals(string.Empty))
            {
                lock (nsusys.MySQLLock)
                {
                    try
                    {
                        using (var cmdsel = nsusys.GetMySqlCmd())
                        {
                            cmdsel.CommandText = "SELECT id FROM `status_names` WHERE `type`=@type AND `name`=@name;";
                            cmdsel.Parameters.Add("@type", MySqlDbType.VarChar).Value = type;
                            cmdsel.Parameters.Add("@name", MySqlDbType.VarChar).Value = name;
                            var dbid = cmdsel.ExecuteScalar();
                            if (dbid == null && insertIfNotExists)
                            {
                                using (var cmdins = nsusys.GetMySqlCmd())
                                {
                                    cmdins.Parameters.Clear();
                                    cmdins.CommandText = "INSERT INTO `status_names`(`type`, `name`) VALUES(@type, @name);";
                                    cmdins.Parameters.Add("@type", MySqlDbType.VarChar).Value = type;
                                    cmdins.Parameters.Add("@name", MySqlDbType.VarChar).Value = name;
                                    cmdins.ExecuteNonQuery();
                                    result = (int)cmdins.LastInsertedId;
                                }
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
            lock (nsusys.MySQLLock)
            {
                try
                {
                    if (dbid != -1)
                    {
                        using (MySqlCommand cmd = nsusys.GetMySqlCmd())
                        {
                            cmd.CommandText = "INSERT INTO `status` (`sid`, `value`) VALUES(@dbid, @value);";
                            cmd.Parameters.Add("@dbid", MySqlDbType.Int32).Value = dbid;
                            cmd.Parameters.Add("@value", MySqlDbType.Int32).Value = value;
                            cmd.ExecuteNonQuery();
                        }
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
            string act = (string)data[JKeys.Generic.Action];
            switch (act)
            {
                case JKeys.Syscmd.Snapshot:
                    if (data.Property(JKeys.Generic.Result) == null)
                    {
                        try
                        {
                            wb = new WoodBoiler();
                            wb.ConfigPos = (byte)data[JKeys.Generic.ConfigPos];
                            wb.Enabled = Convert.ToBoolean((byte)data[JKeys.Generic.Enabled]);
                            wb.Name = (string)data[JKeys.Generic.Name];
                            wb.WorkingTemp = (float)data[JKeys.WoodBoiler.WorkingTemp];
                            wb.Histeresis = (float)data[JKeys.WoodBoiler.Histeresis];
                            wb.TSensorName = (string)data[JKeys.WoodBoiler.TSensorName];
                            wb.KTypeName = (string)data[JKeys.WoodBoiler.KTypeName];
                            wb.ExhaustFanChannel = (byte)data[JKeys.WoodBoiler.ExhaustFanChannel];
                            wb.LadomChannel = (byte)data[JKeys.WoodBoiler.LadomChannel];
                            wb.LadomatTriggerName = (string)data[JKeys.WoodBoiler.LadomatTriggerName];
                            wb.LadomatTemp = (float)data[JKeys.WoodBoiler.LadomatWorkTemp];

                            if (JSonValueOrDefault(data, JKeys.Generic.Content, JKeys.Content.Config) == JKeys.Content.ConfigPlus)
                            {
                                wb.CurrentTemp = (float)data[JKeys.WoodBoiler.CurrentTemp];
                                wb.Status = NSU.Shared.NSUUtils.Utils.GetStatusFromString((string)data[JKeys.WoodBoiler.Status], WoodBoilerStatus.UNKNOWN);
                                wb.LadomStatus = NSU.Shared.NSUUtils.Utils.GetStatusFromString((string)data[JKeys.WoodBoiler.LadomatStatus], LadomatStatus.UNKNOWN);
                                wb.ExhaustFanStatus = NSU.Shared.NSUUtils.Utils.GetStatusFromString((string)data[JKeys.WoodBoiler.ExhaustFanStatus], ExhaustFanStatus.UNKNOWN);
                                wb.TempStatus = NSU.Shared.NSUUtils.Utils.GetStatusFromString((string)data[JKeys.WoodBoiler.TemperatureStatus], WoodBoilerTempStatus.Stable);
                            }

                            wb.AttachXMLNode(nsusys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.WoodBoilers));
                            boilers.Add(wb);
                            //Attach event handler
                            wb.OnExhaustFanStatusChange += WB_OnExhaustFanStatusChange;
                            wb.OnLadomatStatusChange += WB_OnLadomatStatusChange;
                            wb.OnStatusChange += WB_OnStatusChange;
                            wb.OnTempChanged += WB_OnTempChanged;
                        }
                        catch(Exception ex)
                        {
                            NSULog.Exception(LogTag, ""+ex);
                        }
                    }
                    else
                    {
                        if(((string)data[JKeys.Generic.Result]).Equals(JKeys.Result.Done))
                        {
                            foreach(var item in boilers)
                            {
                                item.WBoilerDBId = GetDBID(item.Name, "woodboiler");
                                item.LadomatDBId = GetDBID(item.Name, "ladomat");
                                item.ExhaustFanDBId = GetDBID(item.Name, "exhaust");
                            }
                        }
                    }
                    break;
                case JKeys.Action.Info:
                    if (data.Property(JKeys.Generic.Content) != null)
                    {
                        try
                        {
                            wb = FindWoodBoiler((string)data[JKeys.Generic.Name]);
                            if (wb != null)
                            {
                                string status_str = String.Empty;
                                switch ((string)data[JKeys.Generic.Content])
                                {
                                    case JKeys.WoodBoiler.TargetName:
                                        wb.CurrentTemp = (float)data[JKeys.WoodBoiler.CurrentTemp];
                                        //wb.Status = NSU.Shared.NSUUtils.Utils.GetStatusFromString((string)data[JKeys.WoodBoiler.Status], WoodBoilerStatus.UNKNOWN);
                                                                                
                                        try
                                        {
                                            status_str = (string)data[JKeys.WoodBoiler.Status];
                                            WoodBoilerStatus wbs = NSU.Shared.NSUUtils.Utils.GetStatusFromString(status_str, WoodBoilerStatus.UNKNOWN);
                                            wb.Status = wbs;
                                        }
                                        catch (Exception e)
                                        {
                                            NSULog.Exception(LogTag, $"WoodBoilerStatus GetStatusFromString({status_str}): {e.Message}");
                                        }
                                        wb.TempStatus = NSU.Shared.NSUUtils.Utils.GetStatusFromString((string)data[JKeys.WoodBoiler.TemperatureStatus], WoodBoilerTempStatus.Stable);
                                        break;
                                    case JKeys.WoodBoiler.LadomatStatus:
                                        //wb.LadomStatus = NSU.Shared.NSUUtils.Utils.GetStatusFromString((string)data[JKeys.Generic.Value], LadomatStatus.UNKNOWN);
                                        try
                                        {
                                            status_str = (string)data[JKeys.Generic.Value];
                                            LadomatStatus ls = NSU.Shared.NSUUtils.Utils.GetStatusFromString(status_str, LadomatStatus.UNKNOWN);
                                            wb.LadomStatus = ls;
                                        }
                                        catch (Exception e)
                                        {
                                            NSULog.Exception(LogTag, $"LadomatStatus GetStatusFromString({status_str}): {e.Message}");
                                        }
                                        break;
                                    case JKeys.WoodBoiler.ExhaustFanStatus:
                                        //wb.ExhaustFanStatus = NSU.Shared.NSUUtils.Utils.GetStatusFromString((string)data[JKeys.Generic.Value], ExhaustFanStatus.UNKNOWN);
                                        try
                                        {
                                            status_str = (string)data[JKeys.Generic.Value];
                                            ExhaustFanStatus es = NSU.Shared.NSUUtils.Utils.GetStatusFromString(status_str, ExhaustFanStatus.UNKNOWN);
                                            wb.ExhaustFanStatus = es;
                                        }
                                        catch (Exception e)
                                        {
                                            NSULog.Exception(LogTag, $"LadomatStatus GetStatusFromString({status_str}): {e.Message}");
                                        }
                                        break;
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            NSULog.Exception(LogTag, ex.Message);
                        }
                    }
                    break;
            }
        }

        private void WB_OnTempChanged(WoodBoiler sender, float temp)
        {
            var jo = new JObject();
            jo[JKeys.Generic.Target] = JKeys.WoodBoiler.TargetName;
            jo[JKeys.Generic.Action] = JKeys.Action.Info;
            jo[JKeys.Generic.Name] = sender.Name;
            jo[JKeys.Generic.Content] = JKeys.WoodBoiler.TemperatureChange;
            jo[JKeys.WoodBoiler.TemperatureStatus] = sender.TempStatus.ToString();
            jo[JKeys.WoodBoiler.CurrentTemp] = sender.CurrentTemp;
            SendToClient(NetClientRequirements.CreateStandartAcceptInfo(), jo);
        }

        private void WB_OnStatusChange(WoodBoiler sender, WoodBoilerStatus status)
        {
            InsertStatusValue(sender.WBoilerDBId, (int)status);
            var jo = new JObject();
            jo[JKeys.Generic.Target] = JKeys.WoodBoiler.TargetName;
            jo[JKeys.Generic.Action] = JKeys.Action.Info;
            jo[JKeys.Generic.Name] = sender.Name;
            jo[JKeys.Generic.Content] = JKeys.Generic.Status;
            jo[JKeys.Generic.Value] = status.ToString();
            SendToClient(NetClientRequirements.CreateStandartAcceptInfo(), jo);
        }

        private void WB_OnLadomatStatusChange(WoodBoiler sender, LadomatStatus status)
        {
            InsertStatusValue(sender.LadomatDBId, (int)status);
            var jo = new JObject();
            jo[JKeys.Generic.Target] = JKeys.WoodBoiler.TargetName;
            jo[JKeys.Generic.Action] = JKeys.Action.Info;
            jo[JKeys.Generic.Name] = sender.Name;
            jo[JKeys.Generic.Content] = JKeys.WoodBoiler.LadomatStatus;
            jo[JKeys.Generic.Value] = status.ToString();
            SendToClient(NetClientRequirements.CreateStandartAcceptInfo(), jo);
        }

        private void WB_OnExhaustFanStatusChange(WoodBoiler sender, ExhaustFanStatus status)
        {
            InsertStatusValue(sender.ExhaustFanDBId, (int)status);
            var jo = new JObject();
            jo[JKeys.Generic.Target] = JKeys.WoodBoiler.TargetName;
            jo[JKeys.Generic.Action] = JKeys.Action.Info;
            jo[JKeys.Generic.Name] = sender.Name;
            jo[JKeys.Generic.Content] = JKeys.WoodBoiler.ExhaustFanStatus;
            jo[JKeys.Generic.Value] = status.ToString();
            SendToClient(NetClientRequirements.CreateStandartAcceptInfo(), jo);
        }

        public override void ProccessNetworkData(NetClientData clientData, JObject data)
        {
            try
            {
                WoodBoiler wb = FindWoodBoiler((string)data[JKeys.Generic.Name]);
                switch((string)data[JKeys.Generic.Action])
                {
                    case JKeys.WoodBoiler.ActionIkurimas:
                        //jo = new JObject();
                        //jo[JKeys.Generic.Target] = JKeys.WoodBoiler.TargetName;
                        //jo[JKeys.Generic.Name] = (string)data[JKeys.Generic.Name];
                        //jo[JKeys.Generic.Action] = JKeys.WoodBoiler.ActionIkurimas;
                        //SendToArduino(jo);
                        SendToArduino(data);
                        break;
                    case JKeys.WoodBoiler.ActionSwitch:
                        //jo = new JObject();
                        //jo[JKeys.Generic.Target] = JKeys.WoodBoiler.TargetName;
                        //jo[JKeys.Generic.Name] = (string)data[JKeys.Generic.Name];
                        //jo[JKeys.Generic.Action] = JKeys.WoodBoiler.ActionSwitch;
                        //jo[JKeys.Generic.Value] = (string)data[JKeys];
                        //SendToArduino(jo);
                        SendToArduino(data);
                        break;
                }
            }
            catch(Exception ex)
            {
                NSULog.Exception(LogTag, "ProccessNetworkData: "+ex.Message);
                var jo = new JObject();
                jo[JKeys.Generic.Target] = JKeys.WoodBoiler.TargetName;
                jo[JKeys.Generic.Result] = JKeys.Result.Error;
                jo[JKeys.Generic.Message] = ex.Message;
                jo = nsusys.SetLastCmdID(clientData, jo);
                SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
            }
        }

        public override void Clear()
        {
            boilers.Clear();
        }
    }
}
