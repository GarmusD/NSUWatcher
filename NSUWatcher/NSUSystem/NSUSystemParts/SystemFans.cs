﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NSU.Shared;
using NSU.Shared.NSUSystemPart;
using NSU.Shared.NSUTypes;
using NSUWatcher.NSUWatcherNet;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class SystemFans : NSUSysPartsBase
    {
        private const string LogTag = "SystemFans";

        private readonly List<SystemFan> _systemFans = new List<SystemFan>();

        public SystemFans(NSUSys sys, PartsTypes type) : base(sys, type) {}

        public override void Clear()
        {
            _systemFans.Clear();
        }

        public override void ProccessArduinoData(JObject data)
        {
            string act = (string)data[JKeys.Generic.Action];
            switch (act)
            {
                case JKeys.Syscmd.Snapshot:
                    ProcessActionSnapshot(data);
                    break;
                case JKeys.Generic.Status:
                    ProcessActionStatus(data);
                    break;
            }
        }

        private void ProcessActionSnapshot(JObject data)
        {
            if (data.Property(JKeys.Generic.Result) == null)
            {
                SystemFan sf = new SystemFan();
                sf.ConfigPos = JSonValueOrDefault(data, JKeys.Generic.ConfigPos, SystemFan.INVALID_VALUE);
                sf.Enabled = JSonValueOrDefault(data, JKeys.Generic.Enabled, false);
                sf.Name = JSonValueOrDefault(data, JKeys.Generic.Name, string.Empty);
                sf.TempSensorName = JSonValueOrDefault(data, JKeys.SystemFan.TSensorName, string.Empty);
                sf.MinTemp = JSonValueOrDefault(data, JKeys.SystemFan.MinTemp, 0f);
                sf.MaxTemp = JSonValueOrDefault(data, JKeys.SystemFan.MaxTemp, 0f);

                if (JSonValueOrDefault(data, JKeys.Generic.Content, JKeys.Content.Config) == JKeys.Content.ConfigPlus)
                    sf.CurrentPWM = JSonValueOrDefault(data, JKeys.Generic.Value, (byte)0);

                _systemFans.Add(sf);
            }
        }

        private void ProcessActionStatus(JObject data)
        {
            string name = (string)data[JKeys.Generic.Name] ?? string.Empty;
            if (data[JKeys.Generic.Value] != null && !string.IsNullOrEmpty(name))
            {
                foreach (var item in _systemFans)
                {
                    if (item.Name == name)
                    {
                        item.CurrentPWM = (byte)data[JKeys.Generic.Value];
                    }
                }
            }
        }

        public override void ProccessNetworkData(NetClientData clientData, JObject data)
        {
            //
        }

        public override string[] RegisterTargets()
        {
            return new string[] { JKeys.SystemFan.TargetName };
        }
    }
}
