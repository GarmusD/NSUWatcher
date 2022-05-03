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
                        rm.ConfigPos = JSonValueOrDefault(data, JKeys.Generic.ConfigPos, RelayModule.INVALID_VALUE);
                        rm.Enabled = JSonValueOrDefault(data, JKeys.Generic.Enabled, false);
                        rm.ActiveLow = JSonValueOrDefault(data, JKeys.RelayModule.ActiveLow, false);
                        rm.Inverted = JSonValueOrDefault(data, JKeys.RelayModule.Inverted, false);
                        if(JSonValueOrDefault(data, JKeys.Generic.Content, JKeys.Content.Config) == JKeys.Content.ConfigPlus)
                            rm.Flags = JSonValueOrDefault(data, JKeys.RelayModule.Flags, (byte)0);

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

