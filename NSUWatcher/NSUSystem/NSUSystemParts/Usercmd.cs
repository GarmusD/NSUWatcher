using NSU.Shared.NSUTypes;
using NSUWatcher.NSUWatcherNet;
using Newtonsoft.Json.Linq;
using NSU.Shared;
using System;
using System.Collections.Generic;

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
            NSULog.Debug(LogTag, $"ProccessArduinoData(JObject data:{data})");
            if (data.Property(JKeys.Generic.Value) != null)
            {
                string cmd = (string)data[JKeys.Generic.Value];

                if (cmd.StartsWith("reboot", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    var args = new List<string>(cmd.Split(' '));
                    if (args.Count == 1)
                    {
                        args.Add("hard");
                    }

                    if(args[1].Trim().Equals("hard"))
                    {
                        NSULog.Debug(LogTag, "Rebooting Arduino using comm DTR...");
                        //nsusys.cmdCenter.Stop();
                        //nsusys.cmdCenter.Start(true);
                        nsusys.cmdCenter.SendDTRSignal();
                    }
                    else if (args[1].Trim().Equals("soft"))
                    {
                        NSULog.Debug(LogTag, "Rebooting Arduino (software loop)...");
                        JObject jo = new JObject();
                        jo[JKeys.Generic.Target] = JKeys.Syscmd.TargetName;
                        jo[JKeys.Generic.Action] = JKeys.Syscmd.RebootSystem;
                        jo[JKeys.Generic.Value] = "soft";
                        SendToArduino(jo);
                    }
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
                    ProcessWoodBoilerCmd(cmd);
                    return;
                }

                if (cmd.StartsWith(JKeys.Switch.TargetName, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    ProcessSwitchCmds(cmd);
                    return;
                }

                if (cmd.StartsWith(JKeys.RelayModule.TargetName, StringComparison.InvariantCultureIgnoreCase))
                {
                    ProcessRelayCmds(cmd);
                    return;
                }

                if (cmd.StartsWith(JKeys.TempSensor.TargetName, StringComparison.InvariantCultureIgnoreCase))
                {
                    ProcessTSensorCmds(cmd);
                    return;
                }

                NSULog.Info("UserCMD", $"Unknown command: '{cmd}'");
            }
        }

        private void ProcessWoodBoilerCmd(string cmd)
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
        }

        private void ProcessSwitchCmds(string cmd)
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
        }

        private void ProcessRelayCmds(string cmd)
        {
            string[] parts = cmd.Split(' ');
            if (parts.Length == 3)
            {
                JObject jo = new JObject();
                jo[JKeys.Generic.Target] = JKeys.RelayModule.TargetName;
                if (parts[1].Equals("open", StringComparison.InvariantCultureIgnoreCase))
                {
                    jo[JKeys.Generic.Action] = JKeys.RelayModule.ActionOpenChannel;
                }
                else if (parts[1].Equals("close", StringComparison.InvariantCultureIgnoreCase))
                {
                    jo[JKeys.Generic.Action] = JKeys.RelayModule.ActionCloseChannel;
                }
                else
                {
                    NSULog.Error(LogTag, $"Invalid command: {cmd}");
                }
                byte b;
                if (byte.TryParse(parts[2], out b))
                {
                    jo[JKeys.Generic.Value] = b;
                    SendToArduino(jo);
                }
            }
            else
            {
                NSULog.Error(LogTag, $"Invalid command: {cmd}");
            }
        }

        private void ProcessTSensorCmds(string cmd)
        {
            string[] parts = cmd.Split(' ');
            if (parts.Length > 2)
            {
                if (parts[1].Equals("simulate", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (parts.Length == 4)
                    {
                        JObject jo = new JObject();
                        jo[JKeys.Generic.Target] = JKeys.TempSensor.TargetName;
                        jo[JKeys.Generic.Action] = "simulate";
                        jo[JKeys.Generic.Name] = parts[2];
                        if (float.TryParse(parts[3], out float val))
                        {
                            jo[JKeys.Generic.Value] = val;
                            SendToArduino(jo);
                        }
                    }
                    else
                    {
                        NSULog.Error(LogTag, $"Invalid command: {cmd}");
                    }
                }
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
