using NSU.Shared.NSUTypes;
using NSUWatcher.NSUWatcherNet;
using Newtonsoft.Json.Linq;
using NSU.Shared;
using System;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class Usercmd : NSUSysPartsBase
    {
        private const string LogTag = "UserCMD";
        public Usercmd(NSUSys sys, PartsTypes type) : base(sys, type)
        {
        }

        public override void Clear()
        {
            //
        }

        public override void ProccessArduinoData(JObject data)
        {
            if (data.Property(JKeys.Generic.Value) != null)
            {
                string cmd = (string)data[JKeys.Generic.Value];
                if (cmd.StartsWith("freeze", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    NSULog.Debug(LogTag, "Freezing Arduino...");
                    cmd = (string)data[JKeys.Generic.Value];
                    SendToArduino(cmd);
                    return;
                }

                if (cmd.StartsWith("status", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    NSULog.Debug(LogTag, "Getting system status...");
                    JObject jo = new JObject();
                    jo[JKeys.Generic.Target] = JKeys.Syscmd.TargetName;
                    jo[JKeys.Generic.Action] = JKeys.Syscmd.SystemStatus;
                    SendToArduino(jo);
                    return;
                }

                if (cmd.StartsWith("ikurimas", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    NSULog.Debug(LogTag, "WoodBoiler Ikurimas...");
                    JObject jo = new JObject();
                    jo[JKeys.Generic.Target] = JKeys.WoodBoiler.TargetName;
                    jo[JKeys.Generic.Name] = (string)data[JKeys.Generic.Name];
                    jo[JKeys.Generic.Action] = JKeys.WoodBoiler.ActionIkurimas;
                    SendToArduino(jo);
                    return;
                }
                
                if (cmd.StartsWith(JKeys.WoodBoiler.TargetName, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] parts = cmd.Split(' ');
                    if (parts[1].Equals("workingtemp", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            JObject jo = new JObject();
                            jo[JKeys.Generic.Target] = JKeys.WoodBoiler.TargetName;
                            jo[JKeys.Generic.Name] = "default";
                            jo[JKeys.Generic.Action] = JKeys.Generic.Setup;
                            jo[JKeys.WoodBoiler.WorkingTemp] = float.Parse(parts[2]);
                            SendToArduino(jo);
                        }
                        catch (Exception ex)
                        {
                            NSULog.Error("UserCMD", ex.Message);
                        }
                    }
                    return;
                }

                if (cmd.StartsWith(JKeys.Switch.TargetName, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] parts = cmd.Split(' ');
                    if (parts[2].Equals("click", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            JObject jo = new JObject();
                            jo[JKeys.Generic.Target] = JKeys.Switch.TargetName;
                            jo[JKeys.Generic.Name] = parts[1];
                            jo[JKeys.Generic.Action] = "click";                          
                            SendToArduino(jo);
                        }
                        catch (Exception ex)
                        {
                            NSULog.Error("UserCMD", ex.Message);
                        }
                    }
                    return;
                }

                NSULog.Info("UserCMD", $"Unknown command: '{cmd}'");
            }
        }

        public override void ProccessNetworkData(NetClientData clientData, JObject data)
        {
            
        }

        public override string[] RegisterTargets()
        {
            return new string[] { JKeys.UserCmd.TargetName };
        }
    }
}
