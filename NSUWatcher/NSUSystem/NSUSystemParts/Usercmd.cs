using Newtonsoft.Json.Linq;
using System;
using NSUWatcher.Interfaces.MCUCommands;
using System.CommandLine;
using NSUWatcher.Interfaces;
using NSU.Shared;
using System.CommandLine.Builder;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using Microsoft.Extensions.Logging;
using System.Collections;
using NSUWatcher.Interfaces.NsuUsers;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class Usercmd : NSUSysPartBase
    {
        public override string[] SupportedTargets => new string[] { JKeys.UserCmd.TargetName };

        private readonly RootCommand _rootCommand;
        private readonly Parser _parser;
        private readonly UserConsole _userConsole;

        public Usercmd(NsuSystem sys, ILoggerFactory loggerFactory, INsuSerializer serializer) : base(sys, loggerFactory, serializer, PartType.UserCommand)
        {
            _userConsole = new UserConsole(_logger);
            _rootCommand = new RootCommand("Supported user commands")
            {
                SetupRebootCommand(),
            };
            _parser = new CommandLineBuilder(_rootCommand)
                //.AddMiddleware(async (context, next) =>
                //{
                //    context.Console = new UserConsole(logger);
                //    await next(context);
                //})
                .UseDefaults()
                .Build();
        }

        private Command SetupRebootCommand()
        {
            Command reboot = new Command("reboot");
            var arg = new Argument<string>().FromAmong("soft", "hard");
            reboot.AddArgument(arg);
            reboot.SetHandler((argValue) =>
            {
                ExecuteRebootCommand(argValue);
            }, arg);
            return reboot;
        }

        private void ExecuteRebootCommand(string argument)
        {
            switch (argument)
            {
                case "soft":
                    return;
                case "hard":
                    return;
                default:
                    _logger.LogInformation($"Unsupported argument '{argument}' provided.");
                    break;
            }
        }

        public override void Clear()
        {
            //
        }

        public override bool ProcessCommandFromMcu(IMessageFromMcu command)
        {
            return false;
        }

        public override IExternalCommandResult ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            UserCmdExecCommandContent? cmdContent = _serializer.Deserialize<UserCmdExecCommandContent>(command.Content);
            if(cmdContent != null)
            {
                _parser.Invoke(cmdContent.Value.Command, _userConsole);
            }
            return null;
        }
        /*
        public override void ProccessArduinoData(JObject data)
        {
            _logger.Debug($"ProccessArduinoData(JObject data:{data})");
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
                        _logger.Debug("Rebooting Arduino using comm DTR...");
                        //nsusys.cmdCenter.Stop();
                        //nsusys.cmdCenter.Start(true);
                        _nsuSys.CmdCenter.SendDTRSignal();
                    }
                    else if (args[1].Trim().Equals("soft"))
                    {
                        _logger.Debug("Rebooting Arduino (software loop)...");
                        JObject jo = new JObject
                        {
                            [JKeys.Generic.Target] = JKeys.Syscmd.TargetName,
                            [JKeys.Generic.Action] = JKeys.Syscmd.RebootSystem,
                            [JKeys.Generic.Value] = "soft"
                        };
                        SendToArduino(jo);
                    }
                    return;
                }

                if (cmd.StartsWith("status", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.Debug("Getting system status...");
                    JObject jo = new JObject
                    {
                        [JKeys.Generic.Target] = JKeys.Syscmd.TargetName,
                        [JKeys.Generic.Action] = JKeys.Syscmd.SystemStatus
                    };
                    SendToArduino(jo);
                    return;
                }

                if (cmd.StartsWith("ikurimas", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.Debug("WoodBoiler Ikurimas...");
                    JObject jo = new JObject
                    {
                        [JKeys.Generic.Target] = JKeys.WoodBoiler.TargetName,
                        [JKeys.Generic.Name] = (string)data[JKeys.Generic.Name],
                        [JKeys.Generic.Action] = JKeys.WoodBoiler.ActionIkurimas
                    };
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

                _logger.Warning($"Unknown command: '{cmd}'");
            }
        }
        */

        private void ProcessWoodBoilerCmd(string cmd)
        {
            string[] parts = cmd.Split(' ');
            if (parts[1].Equals("workingtemp", System.StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    JObject jo = new JObject
                    {
                        [JKeys.Generic.Target] = JKeys.WoodBoiler.TargetName,
                        [JKeys.Generic.Name] = "default",
                        [JKeys.Generic.Action] = JKeys.Generic.Setup,
                        [JKeys.WoodBoiler.WorkingTemp] = float.Parse(parts[2])
                    };
                    ///SendToArduino(jo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ProcessWoodBoilerCmd() exception:");
                }
            }
        }

        private void ProcessSwitchCmds(string cmd)
        {
            string[] parts = cmd.Split(' ');
            if (parts[2].Equals("click", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    JObject jo = new JObject
                    {
                        [JKeys.Generic.Target] = JKeys.Switch.TargetName,
                        [JKeys.Generic.Name] = parts[1],
                        [JKeys.Generic.Action] = "click"
                    };
                    ///SendToArduino(jo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ProcessSwitchCmds() exception:");
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
                    _logger.LogError($"Invalid command: {cmd}");
                }
                byte b;
                if (byte.TryParse(parts[2], out b))
                {
                    jo[JKeys.Generic.Value] = b;
                    ///SendToArduino(jo);
                }
            }
            else
            {
                _logger.LogError($"Invalid command: {cmd}");
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
                            ///SendToArduino(jo);
                        }
                    }
                    else
                    {
                        _logger.LogError($"Invalid command: {cmd}");
                    }
                }
            }
        }

        public override IEnumerable GetEnumerator<T>()
        {
            return null;
        }

        private class UserConsole : IConsole
        {
            public IStandardStreamWriter Out => _writer;
            public bool IsOutputRedirected => true;
            public IStandardStreamWriter Error => _errWriter;
            public bool IsErrorRedirected => true;
            public bool IsInputRedirected => true;

            private readonly ConsoleWriter _writer;
            private readonly ConsoleWriter _errWriter;

            public UserConsole(ILogger logger)
            {
                _writer = new ConsoleWriter(logger, false);
                _errWriter = new ConsoleWriter(logger, true);
            }

            private class ConsoleWriter : IStandardStreamWriter
            {
                private readonly ILogger _logger;
                private readonly bool _errWriter;

                public ConsoleWriter(ILogger logger, bool errWriter)
                {
                    _logger = logger;
                    _errWriter = errWriter;
                }

                public void Write(string value)
                {
                    if (_errWriter)
                        _logger.LogError(value);
                    else
                        _logger.LogInformation(value);
                }
            }
        }


    }
}
