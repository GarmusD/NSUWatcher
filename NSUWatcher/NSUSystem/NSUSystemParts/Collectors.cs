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
                        cl.ConfigPos = JSonValueOrDefault(data, JKeys.Generic.ConfigPos, Collector.INVALID_VALUE);
                        cl.Enabled = Convert.ToBoolean(JSonValueOrDefault(data, JKeys.Generic.Enabled, (byte)0));
                        cl.Name = JSonValueOrDefault(data, JKeys.Generic.Name, string.Empty);
                        cl.CircPumpName = JSonValueOrDefault(data, JKeys.Collector.CircPumpName, string.Empty);
                        cl.ActuatorsCount = JSonValueOrDefault(data, JKeys.Collector.ActuatorsCount, (byte)0);

                        var ja = (JArray)data[JKeys.Collector.Valves];
                        string content = JSonValueOrDefault(data, JKeys.Generic.Content, JKeys.Content.Config);
                        for(int i=0; i < cl.ActuatorsCount; i++)
                        {
                            var jo = ja[i] as JObject;
                            cl.actuators[i].Type = (ActuatorType)(byte)jo[JKeys.Collector.ActuatorType];
                            cl.actuators[i].RelayChannel = (byte)jo[JKeys.Collector.ActuatorChannel];
                            if (content == JKeys.Content.ConfigPlus)
                                cl.actuators[i].Opened = JSonValueOrDefault(jo, JKeys.Generic.Status, false);// (bool)jo[JKeys.Generic.Status];
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
                        for(byte i=0; i < col.ActuatorsCount; i++)
                        {
                            col.actuators[i].Opened = Convert.ToBoolean((string)ja[i]);
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

