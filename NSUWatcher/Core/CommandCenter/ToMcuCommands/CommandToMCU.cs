using System;
using Newtonsoft.Json;
using NSU.Shared.Serializer;
using NSUWatcher.Interfaces.MCUCommands;

namespace NSUWatcher.Core.CommandCenter.ToMcuCommands
{
    public class CommandToMCU<T> : ICommandToMCU where T : ICommandToMcuData
    {
        private readonly T _command;
        private readonly Action<string> _sendAction;

        public CommandToMCU(Action<string> sendAction, T command)
        {
            _command = command ?? throw new ArgumentNullException("MCU Command cannot be null.");
            _sendAction = sendAction ?? throw new ArgumentNullException(nameof(sendAction));
        }

        // TODO Use NsuSerializer
        public string Value => JsonConvert.SerializeObject(_command, LowercaseNamingStrategy.LowercaseSettings);

        public void Send()
        {
            _sendAction(Value);
        }
    }
}
