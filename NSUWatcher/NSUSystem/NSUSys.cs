using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Timers;
using Newtonsoft.Json;
using NSUWatcher.DBUtils;
using NSUWatcher.NSUUserManagement;
using NSU.Shared.NSUTypes;
using NSU.Shared.NSUXMLConfig;
using System.IO;
using NSU.Shared;
using NSUWatcher.NSUSystem.NSUSystemParts;
using Newtonsoft.Json.Linq;
using NSUWatcher.NSUWatcherNet;
using NSU.Shared.NSUNet;
using NSUWatcher.Notifications;

namespace NSUWatcher.NSUSystem
{
    public class NSUSys
    {

        readonly string LogTag = "NSUSys";        

        public const int VERSION_MAJOR = 0;
        public const int VERSION_MINOR = 4;

        public const int PROTOCOL_VERSION = 2;

        private const int LOG_EVERY_MINUTES = 15;

        /***************************************************************************/


        /***************************************************************************/

        public class NSUSysPartInfo
        {
            public PartsTypes PartType;
            public string[] AcceptableCmds;
            public NSUSysPartsBase Part;
        };

        public object MySQLLock = new object();

        private CmdCenter cmdCenter;
        private NetServer server = null;
        private DBUtility dbUtility = null;
        private NSUSysPartInfo sysPart;


        private System.Timers.Timer minuteTimer = new System.Timers.Timer(1000 * 60);
        private System.Timers.Timer secondTimer = new System.Timers.Timer(1000);
        private int oldMin;

        private NSUSysPartInfo nsupart;
        private List<NSUSysPartInfo> nsuparts;
        private bool isReady = false;

        public bool IsReady
        {
            get { return isReady; }
        }

        public List<NSUSysPartInfo> NSUParts
        {
            get { return nsuparts; }
        }

        public NSUXMLConfig XMLConfig
        {
            get { return nsuconfig; }
        }
        NSUXMLConfig nsuconfig = null;

        public NSUUsers Users
        {
            get { return nsuUsers; }
        }
        private NSUUsers nsuUsers;

        public PushNotifications PushNotifications
        {
            get; internal set;
        }

        public NSUSys()
        {
            nsuconfig = new NSUXMLConfig();
            nsuconfig.FileName = Path.Combine(Config.Instance().NSUXMLConfigFilePath, Config.Instance().NSUXMLConfigFile);
            
            string connectionString =
                "Server=localhost;" +
                "Database=NSU;" +
                "User ID=NSU;" +
                "Password=p3m2uNXg;" +
                "Pooling=false";

            cmdCenter = cmdcntr;

            dbUtility = new DBUtility(connectionString);

            nsuUsers = new NSUUsers(dbUtility);

            nsuparts = CreateParts();
            //SYSCMD
            sysPart = new NSUSysPartInfo();
            sysPart.PartType = PartsTypes.System;
            sysPart.Part = new Syscmd(this, sysPart.PartType);
            sysPart.AcceptableCmds = sysPart.Part.RegisterTargets();
            nsuparts.Add(sysPart);

            cmdCenter.OnArduinoCrashed += OnArduinoCrashed;
            cmdCenter.OnCmdCenterStarted += OnCmdCenterStarted;
            cmdCenter.OnArduinoDataReceived += OnArduinoDataReceived;

            minuteTimer.Elapsed += MinuteTimer_Elapsed;
            minuteTimer.AutoReset = true;
            secondTimer.Elapsed += SecondTimer_Elapsed;
            oldMin = DateTime.Now.Minute;
            secondTimer.Enabled = true;

            PushNotifications = new PushNotifications();
        }

        private void SecondTimer_Elapsed(object sender, ElapsedEventArgs e)
        {            
            if(oldMin != DateTime.Now.Minute)
            {
                oldMin = DateTime.Now.Minute;
                minuteTimer.Enabled = true;
                MinuteTimer_Elapsed(null, null);
                secondTimer.Enabled = false;
            }            
        }

        private void MinuteTimer_Elapsed(object sender, ElapsedEventArgs e)
        {            
            if(DateTime.Now.Minute % LOG_EVERY_MINUTES == 0)
            {
                var vs = new JObject();
                vs.Add(JKeys.Generic.Action, JKeys.Timer.ActionTimer);
                foreach(var item in nsuparts)
                {
                    item.Part.ProccessArduinoData(vs);
                }
                vs = null;
            }
        }

        private void OnArduinoDataReceived(JObject data)
        {
            try
            {
                var pi = FindPart((string)data[JKeys.Generic.Target]);
                if (pi != null)
                {
                    pi.Part.ProccessArduinoData(data);
                }
                else
                {
                    NSULog.Error(LogTag, "Target not found: " + (string)data[JKeys.Generic.Target]);
                }
            }
            catch (Exception ex)
            {
                NSULog.Exception(LogTag, "OnArduinoDataReceived(" + data.ToString() + ") - " + ex.Message);
            }
        }

        private void OnCmdCenterStarted()
        {
            var vs = new JObject();
            SendToArduino(vs);
            vs.Add(JKeys.Generic.Target, JKeys.Syscmd.TargetName);
            vs.Add(JKeys.Generic.Action, JKeys.Syscmd.SystemStatus);
            SendToArduino(vs);
        }

        private void OnArduinoCrashed()
        {
            //Inform all clients
            isReady = false;
            NSULog.Debug(LogTag, "Stopping NetServer.");
            JObject jo = new JObject();
            jo[JKeys.Generic.Target] = JKeys.Syscmd.TargetName;
            jo[JKeys.Generic.Action] = JKeys.Action.Status;
            jo[JKeys.Generic.Value] = "reboot";
            SendToClient(NetClientRequirements.CreateStandart(), jo);
        }


        public void MakeReady()
        {
            isReady = true;
            if (server == null)
            {
                NSULog.Debug(LogTag, "Starting NetServer.");

                server = new NetServer();
                server.ClientConnected += HandleClientConnected;
                server.ClientDisconnected += HandleClientDisconnected;
                server.OnDataReceived += NetworkDataReceivedHandler;
                server.Start();
            }
            //Inform all clients
            JObject jo = new JObject();
            jo[JKeys.Generic.Target] = JKeys.Syscmd.TargetName;
            jo[JKeys.Generic.Action] = JKeys.Action.Status;
            jo[JKeys.Generic.Value] = "ready";
            SendToClient(NetClientRequirements.CreateStandart(), jo);
        }

        List<NSUSysPartInfo> CreateParts()
        {
            List<NSUSysPartInfo> parts = new List<NSUSysPartInfo>();
            NSUSysPartInfo part;

            //TSensors
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.TSensors;
            part.Part = new TempSensors(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            parts.Add(part);

            //Switches
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.Switches;
            part.Part = new Switches(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            parts.Add(part);

            //Relays
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.RelayModules;
            part.Part = new RelayModules(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            parts.Add(part);

            //TempTriggers
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.TempTriggers;
            part.Part = new TempTriggers(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            parts.Add(part);

            //CircPumps
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.CircPumps;
            part.Part = new CircPumps(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            parts.Add(part);

            //Collectors
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.Collectors;
            part.Part = new Collectors(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            parts.Add(part);

            //ComfortZones
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.ComfortZones;
            part.Part = new ComfortZones(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            parts.Add(part);

            //KTypes
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.KTypes;
            part.Part = new KTypes(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            parts.Add(part);

            //WaterBoiler
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.WaterBoilers;
            part.Part = new WaterBoilers(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            parts.Add(part);

            //Katilas
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.WoodBoilers;
            part.Part = new WoodBoilers(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            parts.Add(part);

            //UserCmd
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.UserCommand;
            part.Part = new Usercmd(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            parts.Add(part);
            return parts;
        }

        void ClearParts()
        {
            NSULog.Debug(LogTag, "Clearing existing NSU Parts...");
            for (int i = 0; i < nsuparts.Count; i++)
            {
                nsuparts[i] = null;
            }
            nsuparts.Clear();
        }

        string ReadCmdID(JObject jo)
        {
            var lastCmdID = string.Empty;
            if(jo.Property(JKeys.Generic.CommandID) != null)
            {
                lastCmdID = (string)jo[JKeys.Generic.CommandID];
            }
            return lastCmdID;
        }

        public JObject SetLastCmdID(NetClientData clientData, JObject jo)
        {
            if (!string.IsNullOrEmpty(clientData.CommandID))
            {
                jo[JKeys.Generic.CommandID] = clientData.CommandID;
                clientData.CommandID = string.Empty;
            }
            return jo;
        }

        public void SendToClient(NetClientRequirements req, JObject valueset, string msg = "")
        {
            if (server != null)
            {
                server.SendString(req, valueset, msg);
            }
        }

        public MySqlCommand GetMySqlCmd()
        {
            return dbUtility.GetMySqlCmd();
        }

        private NSUSysPartInfo FindPart(PartsTypes parttype)
        {
            for (int i = 0; i < nsuparts.Count; i++)
            {
                if (nsuparts[i].PartType == parttype)
                    return nsuparts[i];
            }
            return null;
        }

        private NSUSysPartInfo FindPart(string cmd)
        {
            for (int i = 0; i < nsuparts.Count; i++)
            {
                //for(int j=0; j < nsuparts[i].AcceptableCmds.Count; i++)
                if (nsuparts[i].AcceptableCmds.Contains(cmd))
                    return nsuparts[i];
            }
            NSULog.Debug(LogTag, string.Format("Supported part for sub cmd {0} not found.", cmd));
            return null;
        }

        void HandleClientDisconnected(NetClientData clientData)
        {
            NSULog.Debug(LogTag, string.Format("Client {0} disconnected.", clientData.ClientID.ToString()));
        }

        void HandleClientConnected(NetClientData clientData)
        {
            NSULog.Debug(LogTag, string.Format("Client [{0}] from IP {1} connected.", clientData.ClientID.ToString(), clientData.IPAddress));
            //SendServerInfoToClient(clientData);
        }

        public void Stop()
        {
            NSULog.Debug(LogTag, "NSUSystem.Stop()");
            NSULog.Debug(LogTag, "Stoping NetServer.");
            if (server != null)
            {
                server.Stop();
            }
            NSULog.Debug(LogTag, "Closing database.");
            dbUtility.Dispose();
        }

        void SendServerInfoToClient(NetClientData clientData)
        {
            return;
            try
            {
                if (clientData != null)
                {
                    string srvInfo = string.Format("NSUServer V{0}.{1} PROTOCOL {2}", VERSION_MAJOR, VERSION_MINOR, PROTOCOL_VERSION);
                    server.SendString(NetClientRequirements.CreateStandartClientOnly(clientData), null, srvInfo, true);
                }
            }
            catch(Exception ex)
            {
                NSULog.Exception(LogTag, "SendServerInfoToClient(): "+ex.Message);
            }
        }

        internal void SendServerHandshakeToClient(NetClientData clientData)
        {
            try
            {
                if (clientData != null)
                {
                    NSULog.Debug(LogTag, "Responding to Handshake request.");
                    JObject jo = new JObject();
                    jo[JKeys.Generic.Target] = JKeys.Syscmd.TargetName;
                    jo[JKeys.Generic.Action] = JKeys.SystemAction.Handshake;
                    jo[JKeys.ServerInfo.Name] = "NSUServer";
                    jo[JKeys.ServerInfo.Version] = $"{VERSION_MAJOR}.{VERSION_MINOR}";
                    jo[JKeys.ServerInfo.Protocol] = PROTOCOL_VERSION;
                    jo = SetLastCmdID(clientData, jo);
                    server.SendString(NetClientRequirements.CreateStandartClientOnly(clientData), jo, null, true);
                }
            }
            catch (Exception ex)
            {
                NSULog.Exception(LogTag, "SendServerHandshakeToClient(): " + ex.Message);
            }
        }

        //Network data handler
        void NetworkDataReceivedHandler(object sender, NetServer.DataReceivedArgs args)
        {
            NSULog.Debug(LogTag, $"ProcessClientMessage(datatype = {args.datatype}).");
            if (args.datatype == NetDataType.String)
            {
                string cmd = args.GetAsString();
                if (cmd.StartsWith("{"))//JSON string
                {
                    try
                    {
                        var netData = JsonConvert.DeserializeObject(cmd) as JObject;
                        if (netData == null)
                        {
                            NSULog.Debug(LogTag, $"JsonConvert.DeserializeObject({cmd}) FAILED.");
                            return;
                        }
                        if (netData.Property(JKeys.Generic.Target) == null)
                        {
                            //error
                            var response = new JObject();
                            response.Add(JKeys.Generic.Result, JKeys.Result.Error);
                            response.Add(JKeys.Generic.ErrCode, "inv_msg_format");
                            response.Add(JKeys.Generic.Message, "Unsupported message format. No target.");
                            if (netData.Property(JKeys.Generic.CommandID) != null)
                                response.Add(JKeys.Generic.CommandID, netData[JKeys.Generic.CommandID]);
                            server.SendString(NetClientRequirements.CreateStandartClientOnly(args.ClientData), response, "");
                            return;
                        }
                        if (args.ClientData.ProtocolVersion == 1)
                        {
                            args.ClientData.ProtocolVersion = 2;
                        }
                        args.ClientData.CommandID = ReadCmdID(netData);
                        NSULog.Debug(LogTag, "Looking for part " + (string)netData[JKeys.Generic.Target]);
                        nsupart = FindPart((string)netData[JKeys.Generic.Target]);
                        if (nsupart != null)
                        {
                            nsupart.Part.ProccessNetworkData(args.ClientData, netData);
                        }
                        else
                        {
                            NSULog.Debug(LogTag, "Part [" + (string)netData[JKeys.Generic.Target] + "] NOT FOUND.");
                        }
                    }
                    catch (Exception exception)
                    {
                        NSULog.Error("NSUSys.ClientProcess()", exception.Message);

                        var response = new JObject();
                        response.Add(JKeys.Generic.Result, JKeys.Result.Error);
                        response.Add(JKeys.Generic.ErrCode, "exception");
                        response.Add(JKeys.Generic.Message, exception.Message);
                        server.SendString(NetClientRequirements.CreateStandartClientOnly(args.ClientData), response, "");
                    }
                }
                else
                {
                    NSULog.Debug(LogTag, "Unsupported protocol version. Disconnecting client.");
                    args.ClientData.ProtocolVersion = 1;
                    server.SendString(NetClientRequirements.CreateStandartClientOnly(args.ClientData), null, "UNSUPPORTED PROTOCOL VERSION");
                    server.DisconnectClient(args.ClientData);
                }

            }
            else if (args.datatype == NetDataType.Binary)
            {

            }
        }

        public void SendToArduino(JObject data, bool waitAck = false)
        {
            cmdCenter.SendToArduino(JsonConvert.SerializeObject(data), waitAck);
        }

        public void SendToArduino(string data, bool waitAck = false)
        {
            cmdCenter.SendToArduino(data, waitAck);
        }

    }
}

