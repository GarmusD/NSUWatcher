using System;
using NSU.Shared.NSUTypes;
using System.Collections.Generic;
using NSU.Shared.NSUSystemPart;
using NSU.Shared;
using Newtonsoft.Json.Linq;
using NSUWatcher.NSUWatcherNet;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class Collectors : NSUSysPartsBase
    {
        readonly string LogTag = "Collectors";
        private List<Collector> collectors = new List<Collector>();

        public Collectors(NSUSys sys, PartsTypes type)
            : base(sys, type)
        {
        }

        public override string[] RegisterTargets()
        {
            return new string[] { JKeys.Collector.TargetName, "COLLECTOR:" };
        }

        private Collector FindCollector(string name)
        {
            for (int i = 0; i < collectors.Count; i++)
            {
                if (collectors[i].Name.Equals(name))
                    return collectors[i];
            }
            return null;
        }

        private Collector FindCollector(byte cfg_pos)
        {
            for (int i = 0; i < collectors.Count; i++)
            {
                if (collectors[i].ConfigPos == cfg_pos)
                    return collectors[i];
            }
            return null;
        }

        public override void ProccessArduinoData(JObject data)
        {
            switch ((string)data[JKeys.Generic.Action])
            {                
                case JKeys.Syscmd.Snapshot:
                    if (data.Property(JKeys.Generic.Result) == null)
                    {
                        Collector cl = new Collector();
                        cl.ConfigPos = (int)data[JKeys.Generic.ConfigPos];
                        cl.Name = (string)data[JKeys.Generic.Name];
                        cl.CircPumpName = (string)data[JKeys.Collector.CircPumpName];
                        cl.ValveCount = (byte)data[JKeys.Collector.ValveCount];
                        var ja = (JArray)data[JKeys.Collector.Valves];
                        for(int i=0; i < cl.ValveCount; i++)
                        {
                            var jo = ja[i];
                            cl.Valves[i].Type = (ValveType)(byte)jo[JKeys.Collector.ValveType];
                            cl.Valves[i].RelayChannel = (byte)jo[JKeys.Collector.ValveChannel];
                            cl.Valves[i].Opened = (bool)jo[JKeys.Generic.Status];
                        }
                        cl.AttachXMLNode(nsusys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.Collectors));
                        collectors.Add(cl);
                    }
                    break;
                case JKeys.Action.Info:
                    var col = FindCollector((string)data[JKeys.Generic.Name]);
                    if(col != null)
                    {
                        JArray ja = (JArray)data[JKeys.Generic.Status];
                        for(byte i=0; i < col.ValveCount; i++)
                        {
                            col.Valves[i].Opened = Convert.ToBoolean((string)ja[i]);
                        }
                    }
                    SendToClient(NetClientRequirements.CreateStandartAcceptInfo(), data);
                    break;
            }
        }

        public override void ProccessNetworkData(NetClientData clientData, JObject data)
        {
            try
            {
                //switch ((string)data[JKeys.Generic.Action])
                //{

                //}
            }
            catch (Exception ex)
            {
                NSULog.Exception(LogTag, "ProccessNetworkData(): " + ex.Message);
            }
        }

        public override void Clear()
        {
            collectors.Clear();
        }
    }
}

