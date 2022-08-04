using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NSU.Shared;
using NSU.Shared.NSUTypes;
using NSUWatcher.NSUWatcherNet;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class Console : NSUSysPartsBase
    {
        private const string LogTag = "Console";
        private readonly List<NetClientData> _clients = new List<NetClientData>();

        public Console(NSUSys sys, PartsTypes partType) : base(sys, partType)
        {
            NSULog.Instance.CatchOutput += NSULog_CatchOutput;
            sys.NetServer.ClientDisconnected += NetServer_ClientDisconnected;
        }        

        public override string[] RegisterTargets()
        {
            return new string[] {JKeys.Console.TargetName };
        }

        public override void Clear()
        {
            
        }

        private void NSULog_CatchOutput(object sender, ConsoleEventArgs e)
        {
            if (_clients.Count > 0 && !string.IsNullOrEmpty(e.Message))
            {
                JObject jo = new JObject
                {
                    [JKeys.Generic.Target] = JKeys.Console.TargetName,
                    [JKeys.Generic.Action] = JKeys.Console.ConsoleOut,
                    [JKeys.Generic.Value] = e.Message
                };
                foreach (var item in _clients)
                {
                    SendToClient(NetClientRequirements.CreateStandartClientOnly(item), jo);
                }
            }
        }

        private void NetServer_ClientDisconnected(NetClientData clientData)
        {
            _clients.Remove(clientData);
        }        

        public override void ProccessNetworkData(NetClientData clientData, JObject data)
        {
            switch((string)data[JKeys.Generic.Action])
            {
                case JKeys.Console.Start:
                    ProcessActionStart(clientData);
                    break;

                case JKeys.Console.Stop:
                    ProcessActionStop(clientData);
                    break;

                case JKeys.Console.ManualCommand:
                    ProcessActionManualCommand(data);
                    break;
            }
        }
        
        private void ProcessActionStart(NetClientData clientData)
        {
            NSULog.Debug(LogTag, $"Client {clientData.ClientID} requested console output.");
            if (!_clients.Exists(x => x.ClientID.Equals(clientData.ClientID)))
            {
                _clients.Add(clientData);
                NSULog.Debug(LogTag, $"Client {clientData.ClientID} added to list.");
            }
        }
        
        private void ProcessActionStop(NetClientData clientData)
        {
            _clients.Remove(clientData);
            NSULog.Debug(LogTag, $"Client {clientData.ClientID} stopped console output.");
        }
        
        private void ProcessActionManualCommand(JObject data)
        {
            var cmd = JSonValueOrDefault(data, JKeys.Generic.Value, string.Empty);
            if (!string.IsNullOrEmpty(cmd))
                nsusys.ManualCommand(cmd);
        }

        public override void ProccessArduinoData(JObject data)
        {

        }
    }
}
