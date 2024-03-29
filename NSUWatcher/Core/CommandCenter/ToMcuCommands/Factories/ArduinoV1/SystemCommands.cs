﻿using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.Interfaces.MCUCommands.To;
using NSUWatcher.Core.CommandCenter.ToMcuCommands.Factories.ArduinoV1.Data.System;
using System;
using NSU.Shared;

namespace NSUWatcher.Core.CommandCenter.ToMcuCommands.Factories.ArduinoV1
{
#nullable enable
    public class SystemCommands : IToMcuSystemCommands
    {
        private readonly Action<string> _defaultSendAction;
        
        public SystemCommands(Action<string> sendAction)
        {
            _defaultSendAction = sendAction;
        }

        public ICommandToMCU EmptyCommand()
        {
            return new CommandToMCU<EmptyCommand>(_defaultSendAction, new EmptyCommand());
        }

        public ICommandToMCU GetMcuStatus()
        {
            return new CommandToMCU<EmptyCommand>(_defaultSendAction, new EmptyCommand(JKeys.Syscmd.SystemStatus));
        }

        public ICommandToMCU PauseBoot()
        {
            return new CommandToMCU<EmptyCommand>(_defaultSendAction, new EmptyCommand(JKeys.Syscmd.PauseBoot));
        }

        public ICommandToMCU RequestSnapshot()
        {
            return new CommandToMCU<EmptyCommand>( _defaultSendAction, new EmptyCommand(JKeys.Syscmd.Snapshot));
        }

        public ICommandToMCU SetTime(int year, int month, int day, int hour, int minute, int second, string? commandId)
        {
            return new CommandToMCU<SetTimeData>(_defaultSendAction, new SetTimeData(year, month, day, hour, minute, second, commandId));
        }
    }
#nullable disable
}
