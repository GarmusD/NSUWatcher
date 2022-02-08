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
                        wb.ConfigPos = (int)data[JKeys.Generic.ConfigPos];
                        wb.Name = (string)data[JKeys.Generic.Name];
                        wb.TempSensorName = (string)data[JKeys.WaterBoiler.TempSensorName];
                        wb.CircPumpName = (string)data[JKeys.WaterBoiler.CircPumpName];
                        wb.TempTriggerName = (string)data[JKeys.WaterBoiler.TempTriggerName];
                        wb.ElPowerChannel = (byte)data[JKeys.WaterBoiler.ElPowerChannel];
                        JArray ja = (JArray)data[JKeys.WaterBoiler.PowerData];

                        for (int j = 0; j < WaterBoiler.MAX_WATERBOILER_POWERDATA_COUNT; j++)
                        {
                            JObject powerdata = (JObject)ja[j];
                            wb[j].Enabled = Convert.ToBoolean(powerdata[JKeys.WaterBoiler.PDEnabled]);
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
