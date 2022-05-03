using System;
using NSU.Shared.NSUTypes;
using NSUWatcher.NSUWatcherNet;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using NSU.Shared.NSUSystemPart;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class WaterBoilers : NSUSysPartsBase
    {
        List<WaterBoiler> boilers = new List<WaterBoiler>();
        public WaterBoilers(NSUSys sys, PartsTypes type)
            : base(sys, type)
        {
        }

        public override string[] RegisterTargets()
        {
            return new string[] {JKeys.WaterBoiler.TargetName};
        }

        public override void ProccessArduinoData(JObject data)
        {
            string act = (string)data[JKeys.Generic.Action];
            switch(act)
            {
                case JKeys.Syscmd.Snapshot:
                    if (data.Property(JKeys.Generic.Result) == null)
                    {
                        var wb = new WaterBoiler();
                        wb.ConfigPos = JSonValueOrDefault(data, JKeys.Generic.ConfigPos, WaterBoiler.INVALID_VALUE);
                        wb.Enabled = JSonValueOrDefault(data, JKeys.Generic.Enabled, false);
                        wb.Name = JSonValueOrDefault(data, JKeys.Generic.Name, string.Empty);
                        wb.TempSensorName = JSonValueOrDefault(data, JKeys.WaterBoiler.TempSensorName, string.Empty);
                        wb.CircPumpName = JSonValueOrDefault(data, JKeys.WaterBoiler.CircPumpName, string.Empty);
                        wb.TempTriggerName = JSonValueOrDefault(data, JKeys.WaterBoiler.TempTriggerName, string.Empty);
                        wb.ElHeatingChannel = JSonValueOrDefault(data, JKeys.WaterBoiler.ElPowerChannel, WaterBoiler.INVALID_VALUE);

                        JArray ja = (JArray)data[JKeys.WaterBoiler.PowerData];
                        for (int j = 0; j < WaterBoiler.MAX_WATERBOILER_EL_HEATING_COUNT; j++)
                        {
                            JObject powerdata = (JObject)ja[j];
                            wb[j].StartHour = Convert.ToByte(powerdata[JKeys.WaterBoiler.PDStartHour]);
                            wb[j].StartMin = Convert.ToByte(powerdata[JKeys.WaterBoiler.PDStartMin]);
                            wb[j].EndHour = Convert.ToByte(powerdata[JKeys.WaterBoiler.PDStopHour]);
                            wb[j].EndMin = Convert.ToByte(powerdata[JKeys.WaterBoiler.PDStopMin]);
                        }
                        wb.AttachXMLNode(nsusys.XMLConfig.GetConfigSection(NSU.Shared.NSUXMLConfig.ConfigSection.WaterBoilers));
                        boilers.Add(wb);
                    }
                    break;
                case JKeys.Action.Info:
                    break;
            }
        }

        public override void ProccessNetworkData(NetClientData clientData, JObject data)
        {
            
        }        

        public override void Clear()
        {
            boilers.Clear();
        }
    }
}
