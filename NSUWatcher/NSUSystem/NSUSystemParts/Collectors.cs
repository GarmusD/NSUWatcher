using System;
using System.Linq;
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
        private readonly List<Collector> _collectors = new List<Collector>();

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
            return _collectors.FirstOrDefault(x => x.Name == name);
        }

        private Collector FindCollector(byte cfg_pos)
        {
            return _collectors.FirstOrDefault(x => x.ConfigPos == cfg_pos);
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

                        int idx = 0;
                        foreach (JObject jo in ja)
                        {
                            if (idx < cl.Actuators.Count)
                            {
                                cl.Actuators[idx].Type = (ActuatorType)(byte)jo[JKeys.Collector.ActuatorType];
                                cl.Actuators[idx].RelayChannel = (byte)jo[JKeys.Collector.ActuatorChannel];
                                if (content == JKeys.Content.ConfigPlus)
                                    cl.Actuators[idx].Opened = JSonValueOrDefault(jo, JKeys.Generic.Status, false);// (bool)jo[JKeys.Generic.Status];
                            }
                            idx++;
                        }
                        cl.AttachXMLNode(nsusys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.Collectors));
                        _collectors.Add(cl);
                    }
                    break;

                case JKeys.Action.Info:
                    var col = FindCollector((string)data[JKeys.Generic.Name]);
                    if (col != null)
                    {
                        JArray ja = (JArray)data[JKeys.Generic.Status];
                        int idx = 0;
                        foreach (string boolValue in ja)
                        {
                            if (idx < col.Actuators.Count)
                            {
                                col.Actuators[idx].Opened = Convert.ToBoolean(boolValue);
                            }
                            idx++;
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
            _collectors.Clear();
        }
    }
}

