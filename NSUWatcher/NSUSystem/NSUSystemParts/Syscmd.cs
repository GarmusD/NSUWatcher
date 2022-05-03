using System;
using NSU.Shared.NSUTypes;
using NSU.Shared;
using Newtonsoft.Json.Linq;
using NSU.Shared.NSUNet;
using NSUWatcher.NSUWatcherNet;
using System.Threading;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public partial class Syscmd : NSUSysPartsBase
    {
        readonly string LogTag = "SystemCMD";

        public Syscmd(NSUSys sys, PartsTypes part):base(sys, part)
        {
        }        

        public override string[] RegisterTargets()
        {
            return new string[] {JKeys.Syscmd.TargetName, "SYSCMD:" };
        }

        public override void ProccessArduinoData(JObject data)
        {
            switch ((string)data[JKeys.Generic.Action])
            {                
                case JKeys.Syscmd.SystemStatus:
                    switch ((string)data[JKeys.Generic.Value])
                    {
                        case JKeys.Syscmd.SystemBooting:
                            nsusys.ArduinoStatus = MCUStatus.Booting;
                            break;
                        case JKeys.Syscmd.ReadyPauseBoot:
                            nsusys.ArduinoStatus = MCUStatus.BootPauseReady;
                            if(Config.Instance().ArduinoPauseBoot)
                            {
                                //Clear pause boot flag for next boot
                                Config.Instance().ArduinoPauseBoot = false;
                                PauseBoot();
                            }
                            break;
                        case JKeys.Syscmd.SystemBootPaused:
                            nsusys.ArduinoStatus = MCUStatus.BootPaused;
                            //Inform users about this state
                            break;
                        case JKeys.Syscmd.SystemRunning:
                            nsusys.ArduinoStatus = MCUStatus.Running;
                            SetArduinoClock();
                            Thread.Sleep(2000);
                            ExecuteUserCommands();
                            Thread.Sleep(1500);
                            ExecuteArduinoSnapshot();
                            break;
                    }
                    break;
                case JKeys.Syscmd.Snapshot:
                    var res = (string)data[JKeys.Generic.Result];
                    if(res.Equals(JKeys.Result.Done))
                    {
                        foreach(var item in nsusys.NSUParts)
                        {
                            if(item.PartType != PartsTypes.System)
                            {
                                try
                                {
                                    item.Part.ProccessArduinoData(data);
                                }
                                catch(Exception ex)
                                {
                                    NSULog.Debug(LogTag, item.PartType.ToString() + " Exception: " + ex);
                                }
                            }
                        }
                        nsusys.XMLConfig.Save();
                        nsusys.MakeReady();
                    }
                    break;
                case JKeys.Syscmd.TimeChanged:
                    SetArduinoClock();
                    break;
                case JKeys.Generic.Error:
                    var value = (string)data[JKeys.Generic.Value];
                    switch (value)
                    {
                        case "notjson":
                            //var vs = new JObject();
                            //vs.Add(JKeys.Generic.Target, JKeys.Syscmd.TargetName);
                            //vs.Add(JKeys.Generic.Action, JKeys.Syscmd.SystemStatus);
                            //SendToArduino(vs);
                            break;
                    }
                    break;
            }
        }

        public override void ProccessNetworkData(NetClientData clientData, JObject data)
        {
            switch ((string)data[JKeys.Generic.Action])
            {                
                case JKeys.Syscmd.Snapshot:
                    ActionSnapshot(clientData);
                    break;
                case JKeys.SystemAction.Login:
                    ActionLogin(clientData, data);
                    break;
                case JKeys.SystemAction.Handshake:
                    nsusys.SendServerHandshakeToClient(clientData);
                    break;
                case JKeys.SystemAction.PushID:
                    ActionSetPushID(clientData, data);
                    break;
            }
        }        

        private void SetArduinoClock()
        {
            if (nsusys.SystemTime.TimeIsOk && nsusys.ArduinoStatus == MCUStatus.Running)
            {
                NSULog.Debug(LogTag, "Setting time for MCU...");
                var vs = new JObject();
                vs.Add(JKeys.Generic.Target, JKeys.Syscmd.TargetName);
                vs.Add(JKeys.Generic.Action, JKeys.Syscmd.SetTime);

                DateTime dt = DateTime.Now;
                vs.Add(JKeys.Syscmd.Year, dt.Year);
                vs.Add(JKeys.Syscmd.Month, dt.Month);
                vs.Add(JKeys.Syscmd.Day, dt.Day);
                vs.Add(JKeys.Syscmd.Hour, dt.Hour);
                vs.Add(JKeys.Syscmd.Minute, dt.Minute);
                vs.Add(JKeys.Syscmd.Second, dt.Second);
                SendToArduino(vs);
            }
            else
            {
                NSULog.Debug(LogTag, $"Cant set time for MCU: [nsusys.SystemTime.TimeIsOk: {nsusys.SystemTime.TimeIsOk}, nsusys.ArduinoStatus: {nsusys.ArduinoStatus.ToString()}]");
            }
        }

        private void ExecuteUserCommands()
        {
            foreach (string item in Config.Instance().CmdLines)
            {
                NSULog.Debug(LogTag, "UserCmd: " + item);
                SendToArduino("user "+item);
            }
        }

        private void ExecuteArduinoSnapshot()
        {
            //Clear before new snapshot
            foreach (var item in nsusys.NSUParts)
            {
                item.Part.Clear();                
            }
            nsusys.XMLConfig.Clear();

            var vs = new JObject();
            vs.Add(JKeys.Generic.Target, JKeys.Syscmd.TargetName);
            vs.Add(JKeys.Generic.Action, JKeys.Syscmd.Snapshot);
            SendToArduino(vs);
        }

        private void PauseBoot()
        {
            var vs = new JObject();
            vs.Add(JKeys.Generic.Target, JKeys.Syscmd.TargetName);
            vs.Add(JKeys.Generic.Action, JKeys.Syscmd.PauseBoot);
            SendToArduino(vs);
        }

        private void ActionSnapshot(NetClientData clientData)
        {
            var req = NetClientRequirements.CreateStandartClientOnly(clientData, true, false);
            if (NetClientRequirements.Check(req, clientData))
            {
                nsusys.XMLConfig.Save();
                var jo = new JObject();
                jo[JKeys.Generic.Target] = JKeys.Syscmd.TargetName;
                jo[JKeys.Generic.Action] = JKeys.Syscmd.Snapshot;
                jo[JKeys.Generic.Result] = JKeys.Result.Ok;
                nsusys.SetLastCmdID(clientData, jo);
                jo[JKeys.Generic.Value] = nsusys.XMLConfig.GetXDocAsString();
                nsusys.SendToClient(req, jo);
                clientData.IsReady = true;
            }
        }

        private void ActionLogin(NetClientData clientData, JObject data)
        {
            NSULog.Debug(LogTag, "ActionLogin.");
            switch ((string)data[JKeys.Generic.Content])
            {
                case JKeys.ActionLogin.LoginWithCredentials:
                    NSULog.Debug(LogTag, "ActionLogin: Logging with Credentials.");
                    clientData.UserData = nsusys.Users.Login((string)data[JKeys.ActionLogin.UserName], (string)data[JKeys.ActionLogin.Password]);
                    if(clientData.UserData != null)
                    {
                        clientData.LoggedIn = true;
                        if(data.Property(JKeys.UserInfo.UserAccepts) != null)
                        {
                            clientData.ClientAccepts = (NetClientAccepts)(int)data[JKeys.UserInfo.UserAccepts];
                        }
                        JObject jo = new JObject();
                        jo[JKeys.Generic.Target] = JKeys.Syscmd.TargetName;
                        jo[JKeys.Generic.Action] = JKeys.SystemAction.Login;
                        jo[JKeys.Generic.Result] = JKeys.Result.Ok;
                        jo[JKeys.ActionLogin.DeviceID] = clientData.UserData.DeviceId;
                        jo[JKeys.ActionLogin.Hash] = clientData.UserData.Hash;
                        jo[JKeys.ActionLogin.NeedChangePassword] = clientData.UserData.NeedChangePassword;
                        nsusys.SetLastCmdID(clientData, jo);
                        SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
                    }
                    else
                    {
                        clientData.LoggedIn = false;
                        clientData.ClientAccepts = NetClientAccepts.None;
                        clientData.IsReady = false;

                        JObject jo = new JObject();
                        jo[JKeys.Generic.Target] = JKeys.Syscmd.TargetName;
                        jo[JKeys.Generic.Action] = JKeys.SystemAction.Login;
                        jo[JKeys.Generic.Result] = JKeys.Result.Error;
                        jo[JKeys.Generic.ErrCode] = JKeys.ErrCodes.Login.InvalidUsrNamePassword;
                        nsusys.SetLastCmdID(clientData, jo);
                        SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
                    }
                    return;
                case JKeys.ActionLogin.LoginWithHash:
                    NSULog.Debug(LogTag, "ActionLogin: Logging in with Hash");
                    clientData.UserData = nsusys.Users.LoginUsingHash((string)data[JKeys.ActionLogin.DeviceID], (string)data[JKeys.ActionLogin.Hash]);
                    if (clientData.UserData != null)
                    {
                        clientData.LoggedIn = true;
                        if (data.Property(JKeys.UserInfo.UserAccepts) != null)
                        {
                            clientData.ClientAccepts = (NetClientAccepts)(int)data[JKeys.UserInfo.UserAccepts];
                        }
                        JObject jo = new JObject();
                        jo[JKeys.Generic.Target] = JKeys.Syscmd.TargetName;
                        jo[JKeys.Generic.Action] = JKeys.SystemAction.Login;
                        jo[JKeys.Generic.Result] = JKeys.Result.Ok;
                        jo[JKeys.ActionLogin.DeviceID] = clientData.UserData.DeviceId;
                        jo[JKeys.ActionLogin.Hash] = clientData.UserData.Hash;
                        jo[JKeys.ActionLogin.NeedChangePassword] = clientData.UserData.NeedChangePassword;
                        nsusys.SetLastCmdID(clientData, jo);
                        SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
                    }
                    else
                    {
                        clientData.LoggedIn = false;
                        clientData.ClientAccepts = NetClientAccepts.None;
                        clientData.IsReady = false;

                        JObject jo = new JObject();
                        jo[JKeys.Generic.Target] = JKeys.Syscmd.TargetName;
                        jo[JKeys.Generic.Action] = JKeys.SystemAction.Login;
                        jo[JKeys.Generic.Result] = JKeys.Result.Error;
                        jo[JKeys.Generic.ErrCode] = JKeys.ErrCodes.Login.InvalidHash;
                        nsusys.SetLastCmdID(clientData, jo);
                        SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
                    }
                    return;
            }
        }

        private void ActionSetPushID(NetClientData clientData, JObject data)
        {
            if(!clientData.LoggedIn)
            {
                JObject jo = JResponses.ResultError(JKeys.Syscmd.TargetName, JKeys.SystemAction.PushID, "authentication_required");
                nsusys.SetLastCmdID(clientData, jo);
                SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
                return;
            }
            if (data.Property(JKeys.Generic.Value) != null)
            {
                var pid = (string)data[JKeys.Generic.Value];
                if (!string.IsNullOrEmpty(pid) && nsusys.PushNotifications.ValidateDeviceType(clientData.UserData.DeviceType))
                {
                    var ret = nsusys.Users.SetPushID(clientData.UserData, pid);
                    if(!string.IsNullOrEmpty(ret))
                    {
                        var jo = JResponses.ResultError(JKeys.Syscmd.TargetName, JKeys.SystemAction.PushID, ret);
                    }
                }
            }
        }

        public override void Clear()
        {
            //
        }
    }
}
