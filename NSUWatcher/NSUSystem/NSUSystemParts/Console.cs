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
        private List<NetClientData> clients = new List<NetClientData>();

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
            if (clients.Count > 0 && !string.IsNullOrEmpty(e.Message))
            {
                JObject jo = new JObject
                {
                    [JKeys.Generic.Target] = JKeys.Console.TargetName,
                    [JKeys.Generic.Action] = JKeys.Console.ConsoleOut,
                    [JKeys.Generic.Value] = e.Message
                };
                foreach (var item in clients)
                {
                    SendToClient(NetClientRequirements.CreateStandartClientOnly(item), jo);
                }
            }
        }

        private void NetServer_ClientDisconnected(NetClientData clientData)
        {
            clients.Remove(clientData);
        }        

        public override void ProccessNetworkData(NetClientData clientData, JObject data)
        {
            switch((string)data[JKeys.Generic.Action])
            {
                case JKeys.Console.Start:
                    NSULog.Debug(LogTag, $"Client {clientData.ClientID} requested console output.");
                    if (!clients.Exists(x => x.ClientID.Equals(clientData.ClientID)))
                    {
                        clients.Add(clientData);
                        NSULog.Debug(LogTag, $"Client {clientData.ClientID} added to list.");
                    }
                    break;
                case JKeys.Console.Stop:
                    clients.Remove(clientData);
                    NSULog.Debug(LogTag, $"Client {clientData.ClientID} stopped console output.");
                    break;
                case JKeys.Console.ManualCommand:
                    var cmd = JSonValueOrDefault(data, JKeys.Generic.Value, string.Empty);
                    if (!string.IsNullOrEmpty(cmd))
                        nsusys.ManualCommand(cmd);
                    break;
            }
        }

        public override void ProccessArduinoData(JObject data)
        {

        }
    }
}
