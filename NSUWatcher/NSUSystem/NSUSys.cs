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

        

        public CmdCenter cmdCenter;
        public NetServer NetServer { get; private set; } = null;

        private MySqlDBUtility dbUtility = null;
        private NSUSysPartInfo sysPart;
        private readonly Config _config;
        private System.Timers.Timer oneMinuteTimer = new System.Timers.Timer(1000 * 60);
        private System.Timers.Timer oneSecondTimer = new System.Timers.Timer(1000);
        private int oldMin;

        private NSUSysPartInfo nsupart;
        private bool isReady = false;

        public Config Config => _config;

        public bool IsReady
        {
            get { return isReady; }
        }

        public MCUStatus ArduinoStatus
        {
            get => arduinoStatus;
            set
            {
                if (arduinoStatus != value)
                {
                    arduinoStatus = value;
                }
            }
        }
        private MCUStatus arduinoStatus = MCUStatus.Off;

        public List<NSUSysPartInfo> NSUParts { get; }

        public NSUXMLConfig XMLConfig { get; } = null;

        public NSUUsers Users { get; }

        public PushNotifications PushNotifications
        {
            get; internal set;
        }

        public DaylightSavingTimeHelper SystemTime { get; }

        public NSUSys(Config config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config), "Config object cannot be null.");

            XMLConfig = new NSUXMLConfig
            {
                FileName = Path.Combine(_config.NSUWritablePath, _config.NSUXMLSnapshotFile)
            };

            cmdCenter = new CmdCenter(_config);

            dbUtility = new MySqlDBUtility(MySqlDBUtility.MakeConnectionString(_config));
            Users = new NSUUsers(dbUtility);

            NSULog.Debug(LogTag, "Starting NetServer.");
            NetServer = new NetServer(_config);
            NetServer.ClientConnected += HandleClientConnected;
            NetServer.ClientDisconnected += HandleClientDisconnected;
            NetServer.DataReceived += NetworkDataReceivedHandler;

            //SYSCMD
            sysPart = new NSUSysPartInfo();
            sysPart.PartType = PartsTypes.System;
            sysPart.Part = new Syscmd(this, sysPart.PartType);
            sysPart.AcceptableCmds = sysPart.Part.RegisterTargets();
            NSUParts = new List<NSUSysPartInfo>();
            CreateParts();

            cmdCenter.OnArduinoCrashed += OnArduinoCrashed;
            cmdCenter.OnCmdCenterStarted += OnCmdCenterStarted;
            cmdCenter.OnArduinoDataReceived += OnArduinoDataReceived;

            oneMinuteTimer.Elapsed += MinuteTimer_Elapsed;
            oneMinuteTimer.AutoReset = true;
            oneSecondTimer.Elapsed += SecondTimer_Elapsed;

            PushNotifications = new PushNotifications();

            SystemTime = new DaylightSavingTimeHelper();
            SystemTime.DaylightTimeChanged += SystemTimeChanged;
            SystemTime.Start();
            
            NetServer.Start();
        }

        

        private void SystemTimeChanged(object sender, EventArgs e)
        {
            CalibrateMinuteStart();
            //Reset Arduino Time
            var vs = new JObject
            {
                { JKeys.Generic.Target, JKeys.Syscmd.TargetName },
                { JKeys.Generic.Action, JKeys.Syscmd.TimeChanged }
            };
            OnArduinoDataReceived(vs);
        }

        public bool Start()
        {
            CalibrateMinuteStart();
            return cmdCenter.Start();
        }

        private void CalibrateMinuteStart()
        {
            oldMin = DateTime.Now.Minute;
            oneSecondTimer.Enabled = true;
        }

        private void SecondTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (oldMin != DateTime.Now.Minute)
            {
                oldMin = DateTime.Now.Minute;
                oneMinuteTimer.Enabled = true;
                MinuteTimer_Elapsed(null, null);
                oneSecondTimer.Enabled = false;
            }
        }

        private void MinuteTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now.Minute % LOG_EVERY_MINUTES == 0)
            {
                var vs = new JObject
                {
                    [JKeys.Generic.Action] = JKeys.Timer.ActionTimer
                };
                foreach (var item in NSUParts)
                    item.Part.ProccessArduinoData(vs);
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
            NSULog.Debug(LogTag, "OnArduinoCrashed() Informing clients.");
            JObject jo = new JObject();
            jo[JKeys.Generic.Target] = JKeys.Syscmd.TargetName;
            jo[JKeys.Generic.Action] = JKeys.Action.Status;
            jo[JKeys.Generic.Value] = "reboot";
            SendToClient(NetClientRequirements.CreateStandart(), jo);

            //Rebooting Arduino
            NSULog.Debug(LogTag, "OnArduinoCrashed(): Rebooting Arduino using DTR.");
            cmdCenter.SendDTRSignal();
        }


        public void MakeReady()
        {
            isReady = true;

            //Inform all clients
            JObject jo = new JObject();
            jo[JKeys.Generic.Target] = JKeys.Syscmd.TargetName;
            jo[JKeys.Generic.Action] = JKeys.Action.Status;
            jo[JKeys.Generic.Value] = JKeys.SystemAction.Ready;
            SendToClient(NetClientRequirements.CreateStandart(), jo);
        }

        private void CreateParts()
        {
            NSUSysPartInfo part;

            //SystemCmd
            NSUParts.Add(sysPart);

            //TSensors
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.TSensors;
            part.Part = new TempSensors(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            NSUParts.Add(part);

            //Switches
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.Switches;
            part.Part = new Switches(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            NSUParts.Add(part);

            //Relays
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.RelayModules;
            part.Part = new RelayModules(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            NSUParts.Add(part);

            //TempTriggers
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.TempTriggers;
            part.Part = new TempTriggers(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            NSUParts.Add(part);

            //CircPumps
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.CircPumps;
            part.Part = new CircPumps(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            NSUParts.Add(part);

            //Collectors
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.Collectors;
            part.Part = new Collectors(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            NSUParts.Add(part);

            //ComfortZones
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.ComfortZones;
            part.Part = new ComfortZones(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            NSUParts.Add(part);

            //KTypes
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.KTypes;
            part.Part = new KTypes(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            NSUParts.Add(part);

            //WaterBoiler
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.WaterBoilers;
            part.Part = new WaterBoilers(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            NSUParts.Add(part);

            //Katilas
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.WoodBoilers;
            part.Part = new WoodBoilers(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            NSUParts.Add(part);

            //SystemFan
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.SystemFan;
            part.Part = new SystemFans(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            NSUParts.Add(part);

            //UserCmd
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.UserCommand;
            part.Part = new Usercmd(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            NSUParts.Add(part);

            //BinUploader
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.BinUploader;
            part.Part = new BinUploader(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            NSUParts.Add(part);

            //Console
            part = new NSUSysPartInfo();
            part.PartType = PartsTypes.Console;
            part.Part = new NSUSystemParts.Console(this, part.PartType);
            part.AcceptableCmds = part.Part.RegisterTargets();
            NSUParts.Add(part);
        }

        void ClearParts()
        {
            NSULog.Debug(LogTag, "Clearing existing NSU Parts...");
            NSUParts.Clear();
        }

        string ReadCmdID(JObject jo)
        {
            var lastCmdID = string.Empty;
            if (jo.Property(JKeys.Generic.CommandID) != null)
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
            if (NetServer != null && NetServer.Operational)
            {
                NetServer.SendString(req, valueset, msg);
            }
        }

        public MySqlCommand GetMySqlCmd()
        {
            return dbUtility.GetMySqlCmd();
        }

        private NSUSysPartInfo FindPart(PartsTypes parttype)
        {
            for (int i = 0; i < NSUParts.Count; i++)
            {
                if (NSUParts[i].PartType == parttype)
                    return NSUParts[i];
            }
            return null;
        }

        private NSUSysPartInfo FindPart(string cmd)
        {
            for (int i = 0; i < NSUParts.Count; i++)
            {
                if (NSUParts[i].AcceptableCmds.Contains(cmd))
                    return NSUParts[i];
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

            SystemTime.Stop();

            NSULog.Debug(LogTag, "Stoping NetServer...");
            if (NetServer != null)
            {
                NetServer.Stop();
                NetServer = null;
            }

            NSULog.Debug(LogTag, "Stopping CmdCenter...");
            cmdCenter.Dispose();
            cmdCenter = null;

            NSULog.Debug(LogTag, "Closing database...");
            dbUtility.Dispose();
            NSULog.Debug(LogTag, "NSUSystem.Stopped.");
        }

        void SendServerInfoToClient(NetClientData clientData)
        {
            return;
            try
            {
                if (clientData != null)
                {
                    string srvInfo = string.Format("NSUServer V{0}.{1} PROTOCOL {2}", VERSION_MAJOR, VERSION_MINOR, PROTOCOL_VERSION);
                    NetServer.SendString(NetClientRequirements.CreateStandartClientOnly(clientData), null, srvInfo, true);
                }
            }
            catch (Exception ex)
            {
                NSULog.Exception(LogTag, "SendServerInfoToClient(): " + ex.Message);
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
                    NetServer.SendString(NetClientRequirements.CreateStandartClientOnly(clientData), jo, null, true);
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
            //NSULog.Debug(LogTag, $"ProcessClientMessage(datatype = {args.datatype}).");
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
                            NetServer.SendString(NetClientRequirements.CreateStandartClientOnly(args.ClientData), response, "");
                            return;
                        }
                        if (args.ClientData.ProtocolVersion == 1)
                        {
                            args.ClientData.ProtocolVersion = 2;
                        }
                        args.ClientData.CommandID = ReadCmdID(netData);
                        //NSULog.Debug(LogTag, "Looking for part " + (string)netData[JKeys.Generic.Target]);
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
                        NetServer.SendString(NetClientRequirements.CreateStandartClientOnly(args.ClientData), response, "");
                    }
                }
                else
                {
                    NSULog.Debug(LogTag, "Unsupported protocol version. Disconnecting client.");
                    args.ClientData.ProtocolVersion = 1;
                    NetServer.SendString(NetClientRequirements.CreateStandartClientOnly(args.ClientData), null, "UNSUPPORTED PROTOCOL VERSION");
                    NetServer.DisconnectClient(args.ClientData);
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

        public void ManualCommand(string cmd)
        {
            NSULog.Debug(LogTag, $"ManualCommand received: {cmd}");
            var vs = new JObject();
            vs.Add(JKeys.Generic.Target, JKeys.UserCmd.TargetName);
            vs.Add(JKeys.Generic.Value, cmd);
            OnArduinoDataReceived(vs);
        }
    }
}

