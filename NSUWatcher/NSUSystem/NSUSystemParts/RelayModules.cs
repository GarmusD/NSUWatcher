using System;
using NSU.Shared.NSUTypes;
using NSUWatcher.NSUWatcherNet;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using NSU.Shared.NSUSystemPart;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class RelayModules : NSUSysPartsBase
    {
        readonly string LogTag = "RelayModules";

        List<RelayModule> modules = new List<RelayModule>();
        public RelayModules(NSUSys sys, PartsTypes type)
            : base(sys, type)
        {
        }

        public override string[] RegisterTargets()
        {
            return new string[] { JKeys.RelayModule.TargetName };
        }

        public override void ProccessArduinoData(JObject data)
        {
            switch ((string)data[JKeys.Generic.Action])
            {
                case JKeys.Syscmd.Snapshot:
                    if(data.Property(JKeys.Generic.Result) == null)
                    {
                        var rm = new RelayModule();
                        rm.ConfigPos = (int)data[JKeys.Generic.ConfigPos];
                        rm.ActiveLow = Convert.ToBoolean((int)data[JKeys.RelayModule.ActiveLow]);
                        rm.Inverted = Convert.ToBoolean((int)data[JKeys.RelayModule.Inverted]);
                        rm.Flags = (byte)data[JKeys.RelayModule.Flags];

                        rm.AttachXMLNode(nsusys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.RelayModules));
                        modules.Add(rm);
                    }
                    break;
            }
        }

        public override void ProccessNetworkData(NetClientData clientData, JObject data)
        {
            
        }

        public override void Clear()
        {
            modules.Clear();
        }
    }
}

