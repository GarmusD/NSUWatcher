using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.CommandCenter.MessagesFromMcu.Factories;
using NSUWatcher.Core.CommandCenter.ToMcuCommands.Factories;
using System;
using Microsoft.Extensions.Logging;

namespace NSUWatcher.CommandCenter
{
    public class MCUCommands : IMcuCommands
    {
        public IToMcuCommands ToMcu { get => _toMcuCommands; set => SetToMcuCommands(value); }
        public IFromMcuMessages FromMcu { get => _messagesFromMcu; set => SetMessagesFromMcu(value); }

        private IFromMcuMessages _messagesFromMcu;
        private IToMcuCommands _toMcuCommands;

        public MCUCommands(Action<string> sendAction, ILoggerFactory loggerFactory)
        {
            _messagesFromMcu = MessageFromMcuFactories.GetDefault(loggerFactory);
            _toMcuCommands = ToMcuCommandFactories.GetDefault(sendAction);
        }
        
        private void SetToMcuCommands(IToMcuCommands value)
        {
            _toMcuCommands = value ?? throw new ArgumentNullException(nameof(value), "IToMcuCommands cannot be null.");
        }
        
        private void SetMessagesFromMcu(IFromMcuMessages value)
        {
            _messagesFromMcu = value ?? throw new ArgumentNullException(nameof(value), "IFromMcuMessages cannot be null.");
        }
    }
}
