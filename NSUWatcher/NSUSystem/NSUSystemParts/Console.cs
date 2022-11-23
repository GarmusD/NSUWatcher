using System;
using NSU.Shared;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.NSUSystemPart;
using NSU.Shared.Serializer;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.Serilog;
using Serilog;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class Console : NSUSysPartBase
    {
        public override string[] SupportedTargets => new string[] { JKeys.Console.TargetName };

        private readonly ConsolePart _console = new ConsolePart();

        public Console(NsuSystem sys, ILogger logger, INsuSerializer serializer) : base(sys, logger, serializer, PartType.Console)
        {
            NsuConsoleMessage.OutputReceived += (s, e) =>
            {
                 _console.Output = e.Output;
            };


            _console.PropertyChanged += (s, e) => 
            {
                _nsuSys.OnStatusChanged(_console, e.PropertyName);
            };
        }

        public override void ProcessCommandFromMcu(IMessageFromMcu command)
        {
            _logger.Warning($"Console Part does not support messages from MCU [{command.GetType().Name}].");
        }

        public override void Clear()
        {
            
        }

        public override IExternalCommandResult? ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            switch(command.Action)
            {
                case JKeys.Console.Start:
                    _console.ContextList.Add(context);
                    return null;

                case JKeys.Console.Stop:
                    _console.ContextList.Remove(context);
                    return null;

                case JKeys.Console.ManualCommand:
                    UserCmdExecCommandContent? content = _serializer.Deserialize<UserCmdExecCommandContent>(command.Content);
                    if (content == null)
                        throw new InvalidCastException($"Invalid content: '{command.Content}'");

                    _nsuSys.CmdCenter.ExecExternalCommand(
                        _nsuSys.CmdCenter.ExternalCommands.UserCmdCommands.ExecUserCommand(content.Value.Command), 
                        nsuUser, context);
                    return null;
            }
            LogNotImplementedCommand(command);
            return null;
        }
    }
}
